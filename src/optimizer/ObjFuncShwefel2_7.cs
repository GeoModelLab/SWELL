// Decompiled with JetBrains decompiler
// Type: UNIMI.optimizer.ObjFuncShwefel2_7
// Assembly: Optimizer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9A749CE1-E04E-4D6A-9AB7-E7251775AF42
// Assembly location: C:\Users\simoneugomaria.brega\Documents\gitProjects\swell\SWELL\runner\DLLs\Optimizer.dll

using System;

#nullable disable
namespace UNIMI.optimizer
{
  public class ObjFuncShwefel2_7 : IOBJfunc
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
      if (Coefficient[2] <= limits[2, 0] | Coefficient[2] > limits[2, 1])
      {
        ++this._neval;
        return 1E+300;
      }
      ++this._neval;
      ++this._ncompute;
      double num = 0.0;
      for (int index = 1; index < 11; ++index)
        num += Math.Pow(Math.Exp((double) -index * Coefficient[0] / 10.0) - Math.Exp((double) -index * Coefficient[1] / 10.0) - (Math.Exp((double) -index / 10.0) - Math.Exp((double) -index)) * Coefficient[2], 2.0);
      return num;
    }
  }
}
