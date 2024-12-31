// Decompiled with JetBrains decompiler
// Type: UNIMI.optimizer.ObjFuncGriewank10
// Assembly: Optimizer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9A749CE1-E04E-4D6A-9AB7-E7251775AF42
// Assembly location: C:\Users\simoneugomaria.brega\Documents\gitProjects\swell\SWELL\runner\DLLs\Optimizer.dll

using System;

#nullable disable
namespace UNIMI.optimizer
{
  public class ObjFuncGriewank10 : IOBJfunc
  {
    private int _neval = 0;
    private int _ncompute = 0;

    public int neval
    {
      get => this._neval;
      set => this._neval = value;
    }

    public int ncompute
    {
      get => this._ncompute;
      set => this._ncompute = value;
    }

    public double ObjfuncVal(double[] Coefficient, double[,] limits)
    {
      for (int index = 0; index < 10; ++index)
      {
        if (Coefficient[index] <= limits[index, 0] | Coefficient[index] > limits[index, 1])
          return 1E+300;
      }
      ++this._neval;
      ++this._ncompute;
      double num1 = 0.0;
      double num2 = 0.0;
      for (int d = 1; d < 11; ++d)
      {
        num1 += Math.Pow(Coefficient[d - 1] / 4000.0, 2.0);
        num2 *= Math.Cos(Coefficient[d - 1] / Math.Sqrt((double) d)) + 1.0;
      }
      return num1 - num2;
    }
  }
}
