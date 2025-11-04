using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ycToolkit.Mina;

public class FileFormatBase
{
    public FileFormatHeader Header = new FileFormatHeader();

    public FileFormatBase()
    {

    }


    public bool Read(string filepath)
    {
        using FileStream fs = new(filepath, FileMode.Open, FileAccess.Read);
        using ycBinaryReader br = new(fs);
        var result = Read(br);
        br.Close();

        return result;
    }
    

    public virtual bool Read(ycBinaryReader br)
    {
        if (!Header.Read(br))
        {
            Debug.WriteLine("Error reading FileFormatHeader");
            return false;
        }
        return true;
    }

}
