using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UNIMI.optimizer
{
    public interface IOBJfunc
    {
        int neval { get; set; }

        int ncompute { get; set; }

        double ObjfuncVal(double[] Coefficient, double[,] limits);
    }
}
