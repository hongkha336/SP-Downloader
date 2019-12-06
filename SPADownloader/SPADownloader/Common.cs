using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPADownloader
{
    public class Common
    {
        /// <summary>
        /// Save file to some PATH
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="path">The path should using \\ partern instead of / partern</param>
        /// <param name="content"></param>
        public static void SaveFile (String fileName, String path, String content)
        {
            //progress - path
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            //progress - filename
            if (!path.EndsWith("\\"))
            {
                path = path + "\\";
            }
            path = path + fileName;

            using (StreamWriter bw = new StreamWriter(File.Create(path)))
            {
                bw.Write(content);
                bw.Close();
            }
        }
        /// <summary>
        /// Save file just with path incude fileName
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static void SaveFile(String path, String content)
        {
            using (StreamWriter bw = new StreamWriter(File.Create(path)))
            {
                bw.Write(content);
                bw.Close();
            }
        }
    }
}
