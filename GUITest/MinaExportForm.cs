using Microsoft.WindowsAPICodePack.Dialogs;
using System.ComponentModel;
using System.Diagnostics;
using ycToolkit;
using ycToolkit.Core;

namespace GUITest
{
    public partial class MinaExportForm : Form
    {
        public class BackgroundWorkerProcArgs(BackgroundWorkerProc proc, object? argument)
        {
            public BackgroundWorkerProc Proc = proc;
            public object? Argument = argument;
        }

        public delegate void BackgroundWorkerProc(BackgroundWorker sender, DoWorkEventArgs args);


        public static void UnpakPakProc(BackgroundWorker sender, DoWorkEventArgs args)
        {
            BackgroundWorkerProcArgs procArgs = args.Argument! as BackgroundWorkerProcArgs;

            if (args.Argument is null)
            {
                args.Result = false;
                return;
            }
            if (args.Argument is not string)
            {
                args.Result = false;
                return;
            }


            var minaSourceDirectory = (string)args.Argument!;
            var files = Directory.GetFiles(minaSourceDirectory, "*.pak.yc");

            for (var i = 0; i < files.Length; i++)
            {
                using FileStream fs = new FileStream(files[i], FileMode.Open, FileAccess.Read);
                using ycToolkit.ycBinaryReader br = new ycToolkit.ycBinaryReader(fs);
                ycToolkit.Mina.PakFormat? pak = new ycToolkit.Mina.PakFormat();


                if (!pak.Read(br))
                {
                    Console.WriteLine("There was an error in unpaking!");
                    continue;
                }




                exportPakFiles(pak, br);

                br.Close();

                sender.ReportProgress((int)(((float)(i + 1) / (float)files.Length) * 100.0f));
            }

            args.Result = true;

            return;

            void exportPakFiles(ycToolkit.Mina.PakFormat pak, ycToolkit.ycBinaryReader br)
            {
                if (pak.FileInfos.Count <= 0)
                {
                    Console.WriteLine("Found no files in pak");
                    return;
                }

                Console.WriteLine($"Pak has {pak.FileInfos.Count} files");

                for (var i = 0; i < pak.FileInfos.Count; i++)
                {

                    var fileInfo = pak.FileInfos[i];
                    var str = pak.tempFileNames[i];


                    var dir = $"ExportData/{Path.GetDirectoryName(str)}";
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    br.BaseStream.Position = fileInfo.FileAddress;

                    var outputName = $"ExportData/{str}";

                    var size = (int)fileInfo.FileSize;
                    Console.WriteLine($"Writing \"{outputName}\"");
                    File.WriteAllBytes(outputName, br.ReadBytes((int)fileInfo.FileSize));
                }
            }
        }

        public static void PakDirectoryProc(DoWorkEventArgs args)
        {

            args.Result = true;
        }

        public MinaExportForm()
        {
            InitializeComponent();
        }

        private void extractMinaPakFilesButton_Click(object sender, EventArgs e)
        {
            string minaSourceDirectory = string.Empty;

            using (CommonOpenFileDialog dlg = new CommonOpenFileDialog())
            {
                dlg.IsFolderPicker = true;
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    minaSourceDirectory = dlg.FileName;
                }
            }

            if (!Directory.Exists("ExportData"))
                Directory.CreateDirectory("ExportData");

            UnpackDataDirectory(minaSourceDirectory);




        }
        private void dirToPakButton_Click(object sender, EventArgs e)
        {

        }

        private void extractMinaANBFilesButton_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists("ExportData"))
            {
                MessageBox.Show("Unable to find \'ExportData\'! Make sure to extract the data first!");
                return;
            }

            // Caster: this code is awful but it will do for now

            var files = Directory.GetFiles("ExportData", "*.anb.yc", SearchOption.AllDirectories);
            Console.WriteLine($"Found: {files.Length} files for Mina");

            int wflzBlockCount = 0;

            foreach (var file in files)
            {
                using FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                using ycBinaryReader br = new ycBinaryReader(fs);

                ycToolkit.Mina.AnbFormat? anb = null;
                var rawfilepath = file.Substring("ExportData".Length + 1);

                ycToolkit.Mina.AnbFormat.Read(br, out anb);

                if (anb.PalettePath.Length == 0)
                    Console.WriteLine($"\"{file}\" has no reported palette?");


                var dir = $"anbExport\\{rawfilepath}";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                Console.WriteLine($"Extracting WFLZ from: \"{rawfilepath}\"");
                byte[] workMem = new byte[wfLZ.GetWorkMemSize()];

                ycToolkit.Mina.PalFormat? pal = null;


                if (anb.PalettePath != "")
                {
                    if (!File.Exists($"ExportData\\{anb.PalettePath}"))
                    {
                        Console.WriteLine($"Unable to properly render: \"{rawfilepath}\" becuase it's palette: \"{anb.PalettePath}\" is missing!");
                        continue;
                    }
                    ycToolkit.Mina.PalFormat.Read($"ExportData\\{anb.PalettePath}", out pal);
                }


                for (var i = 0; i < anb.WFLZBlocks.Count; i++)
                {
                    var wflzAddr = anb.WFLZBlocks[i].Address;
                    var wflzSize = anb.WFLZBlocks[i].Size;
                    var frameInfo = anb.FrameEntries[i];


                    // I know this code is terrible
                    // just bear with me for a bit <3





                    br.BaseStream.Position = anb.WFLZBlocks[i].Address;

                    byte[] source = br.ReadBytes((int)anb.WFLZBlocks[i].Size);

                    byte[] dst = new byte[wfLZ.GetDecompressedSize(source)];

                    if (dst.Length == 0)
                    {
                        Console.WriteLine($"Invalid wfLZ from: \"{rawfilepath}\" -- {anb.WFLZBlocks[i].Address:X4}");
                        continue;
                    }

                    Console.Write($"Decompressing: {i}/{anb.WFLZBlocks.Count} - {anb.WFLZBlocks[i].Address:X4}\r");
                    wfLZ.Decompress(source, dst);


                    //Console.Write($"\n");

                    if (!Directory.Exists($"{dir}/WFLZ"))
                        Directory.CreateDirectory($"{dir}/WFLZ");

                    wflzBlockCount++;
                    File.WriteAllBytes($"{dir}/WFLZ/{anb.WFLZBlocks[i].Address:X4}", dst);

                    using Bitmap bmp = new Bitmap((int)(frameInfo.Width * (pal is null ? 4 : 1)), (int)(frameInfo.Height * (pal is null ? 4 : 1)));

                    for (var y = 0; y < bmp.Height; y++)
                    {
                        for (var x = 0; x < bmp.Width; x++)
                        {
                            var offset = (y * bmp.Width) + x;

                            Color c = Color.Transparent;

                            if (pal is null)
                            {
                                if (offset + 3 < dst.Length)
                                    bmp.SetPixel(x, y, Color.FromArgb(dst[offset + 3], dst[offset + 0], dst[offset + 1], dst[offset + 2]));
                            }
                            else
                            {
                                // if (offset < dst.Length)
                                bmp.SetPixel(x, y, pal.Colors[dst[offset]]);
                            }

                        }
                    }

                    bmp.Save($"{dir}//{anb.WFLZBlocks[i].Address:X4}.png", System.Drawing.Imaging.ImageFormat.Png);
                    //Createbitmap?.Invoke(null, new tempEventArgs(frameInfo.Width, frameInfo.Height, anbFormat.WFLZBlocks[i].Address, dst));
                }
                br.Close();
            }

            Console.WriteLine("================ FINISHED TASK ================");
            Console.WriteLine($"Successfully parsed all {files.Length} anb files in format: MinaTheHollower");
            Console.WriteLine($"WFLZ Block Count: {wflzBlockCount}");
        }

        private void exportMinaPakFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Mina pak file (*.pak.yc)|*.pak.yc";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ycToolkit.Mina.PakFormat pak = new();

                    if (!pak.Read(dlg.FileName))
                    {
                        Debug.WriteLine($"Unable to read: \"{dlg.FileName}\"");
                        return;
                    }
                    Console.WriteLine($"Found {pak.tempFileNames.Count} files! {pak.tempFileNames.Count:X4}");

                }
            }
        }



        private void UnpackDataDirectory(string filepath)
        {
            if (!Directory.Exists(filepath))
                return;

            extractMinaPakFilesButton.Enabled = false;
            pakBackgroundWorker.RunWorkerAsync(new BackgroundWorkerProcArgs(UnpakPakProc, filepath));

        }


        #region Background Worker
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
            if (sender is not BackgroundWorker worker)
            {
                return;
            }

            if (e.Argument is null)
            {
                e.Result = false;
                return;
            }

            if (e.Argument is not BackgroundWorkerProcArgs args)
            {
                e.Result = false;
                return;
            }

            args.Proc(worker, e);
        }


        private void pakBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            extractMinaPakFilesButton.Enabled = true;

            Process.Start("explorer.exe", "ExportData");
        }
        #endregion


        private void BackgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            var toMax = ((float)e.ProgressPercentage / 100.0f);

            progressBar.Value = (int)((float)progressBar.Maximum * toMax);
        }


    }
}
