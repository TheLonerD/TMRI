using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TMRI.UI.Console.Menu
{
    public class Menu
    {
        private readonly List<Option> _options;
        private Func<int> _readInput;

        public Menu()
        {
            _options = new List<Option>();
        }

        public Menu AddInput(Func<int> input)
        {
            _readInput = input;

            return this;
        }

        public async Task<bool> Draw()
        {
            for (int i = 0; i < _options.Count; i++)
            {
                System.Console.WriteLine($"{i + 1}. {_options[i]}");
            }

            if (_readInput != null)
            {
                var input = _readInput();

                return _options[input - 1].Func != null && await _options[input - 1].Func();
            }

            return true;
        }

        public Menu Add(string caption, Func<Task<bool>> func = null)
        {
            if (string.IsNullOrEmpty(caption))
            {
                throw new ArgumentNullException(nameof(caption));
            }

            return Add(new Option(caption, func));
        }

        public Menu Add(Option option)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }
            
            _options.Add(option);

            return this;
        }
    }
}
