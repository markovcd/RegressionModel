﻿using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Markovcd.Classes;

namespace Markovcd
{
    class Program
    { 
        static void Main()
        {
            var y = new[]
            {
                4d, 14, 38, 76, 128, 194, 274, 368, 476, 598, 734, 11, 22, 47, 86, 139, 206, 287, 382, 491, 614, 751, 28,
                40, 66, 106, 160, 228, 310, 406, 516, 640, 778, 55, 68, 95, 136, 191, 260, 343, 440, 551, 676, 815, 92,
                106, 134, 176, 232, 302, 386, 484, 596, 722, 862, 139, 154, 183, 226, 283, 354, 439, 538, 651, 778, 919,
                196, 212, 242, 286, 344, 416, 502, 602, 716, 844, 986, 263, 280, 311, 356, 415, 488, 575, 676, 791, 920,
                1063, 340, 358, 390, 436, 496, 570, 658, 760, 876, 1006, 1150, 427, 446, 479, 526, 587, 662, 751, 854,
                971, 1102, 1247, 524, 544, 578, 626, 688, 764, 854, 958, 1076, 1208, 1354
            };
            var p1 = new[]
            {
                0d, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3,
                3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6,
                6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 9, 9,
                9, 9, 9, 9, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10
            };
            var p2 = new[]
            {
                0d, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 1,
                2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 1, 2,
                3, 4, 5, 6, 7, 8, 9, 10, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 1, 2, 3,
                4, 5, 6, 7, 8, 9, 10, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
            };
            
            
            var funcStr = "f(x1, x2) = x1 + x2 + x1^2 + x2^2 + x1*x2 + Sin(x1)";
            //var model = new ModelParser(funcStr, y, p1, p2);
            //var model = Model.Create((x1, x2) => x1 + x2 + x1*x1 + x2*x2 + x1*x2 , y, p1, p2);
            var filename = @"C:\Users\Arek\Desktop\aproksymacja 3d.xlsx";
            //var model = new ExcelModel(filename, "dane", funcStr, "A2:A122", "B2:C122");
            //model.WriteResults("dane", "J10");
            //model.Dispose();


         
        }
    }
}
