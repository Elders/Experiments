using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;

namespace SpeechRecognition
{
    class Program
    {
        static void Main(string[] args)
        {
            SpeechRecognizer sr = new SpeechRecognizer();
            Choices colors = new Choices();
            colors.Add(new string[] { "red", "green", "blue" });

            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(colors);

            // Create the Grammar instance.
            Grammar g = new Grammar(gb);
            sr.LoadGrammar(g);
            sr.SpeechRecognized += sr_SpeechRecognized;
            Console.ReadLine();
        }

        static void sr_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine(e.Result.Text);
        }
    }
}
