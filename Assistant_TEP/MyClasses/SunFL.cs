using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant_TEP.MyClasses
{
    /// <summary>
    /// формування файлу оплат за сонячну електроенергію у дбф файл згідно вигрузки
    /// </summary>
    public class SunFL
    {
        public string kb_a { get; set; }
        public string kk_a { get; set; }
        public string kb_b { get; set; }
        public string kk_b { get; set; }
        public string d_k { get; set; }
        public string summa { get; set; }
        public string vid { get; set; }
        public string ndoc { get; set; }
        public string i_va { get; set; }
        public string da { get; set; }
        public string da_doc { get; set; }
        public string nk_a { get; set; }
        public string nk_b { get; set; }
        public string nazn { get; set; }
        public string nazn1 { get; set; }
        public string kod_a { get; set; }
        public string kod_b { get; set; }

        public override string ToString()
        {
            string res = String.Format(
                $"<Sunf {kb_a} {kk_a} {kb_b} {kk_b} {d_k} {summa} {vid} {i_va} {nazn} {kod_b} {da_doc}>"
            );
            return res;
        }

    }
}
