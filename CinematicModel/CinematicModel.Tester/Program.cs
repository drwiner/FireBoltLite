using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CinematicModel;
namespace CinematicModel.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.Write("enter a filename to parse: ");
            CinematicModel cm = Parser.Parse("../../../../FireBoltUnity/cinematicModels/defaultModel.xml");
        }
    }
}
