// Decompiled with JetBrains decompiler
// Type: UNIMI.optimizer.ObjFuncRastrigin
// Assembly: Optimizer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9A749CE1-E04E-4D6A-9AB7-E7251775AF42
// Assembly location: C:\Users\simoneugomaria.brega\Documents\gitProjects\swell\SWELL\runner\DLLs\Optimizer.dll

using System;

#nullable disable
namespace UNIMI.optimizer
{
  public class ObjFuncRastrigin : IOBJfunc
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
      if (Coefficient[0] <= limits[0, 0] | Coefficient[0] > limits[0, 1])
      {
        ++this._neval;
        return 1E+300;
      }
      if (Coefficient[1] <= limits[1, 0] | Coefficient[1] > limits[1, 1])
      {
        ++this._neval;
        return 1E+300;
      }
      ++this._neval;
      ++this._ncompute;
      return 20.0 + Coefficient[0] * Coefficient[0] + Coefficient[1] * Coefficient[1] - 10.0 * (Math.Cos(2.0 * Math.PI * Coefficient[0]) + Math.Cos(2.0 * Math.PI * Coefficient[1]));
    }
  }
}
