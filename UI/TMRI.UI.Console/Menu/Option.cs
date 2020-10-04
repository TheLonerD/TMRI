using System;
using System.Threading.Tasks;

namespace TMRI.UI.Console.Menu
{
    public class Option
    {
        public string Caption { get; set; }

        public Func<Task<bool>> Func { get; set; }
        
        public Option(string caption, Func<Task<bool>> func)
        {
            Caption = caption;
            Func = func;
        }

        public override string ToString() => Caption;
    }
}
