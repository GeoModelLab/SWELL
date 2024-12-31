// Decompiled with JetBrains decompiler
// Type: UNIMI.optimizer.Simplex
// Assembly: Optimizer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9A749CE1-E04E-4D6A-9AB7-E7251775AF42
// Assembly location: C:\Users\simoneugomaria.brega\Documents\gitProjects\swell\SWELL\runner\DLLs\Optimizer.dll

using System;

#nullable disable
namespace UNIMI.optimizer
{
  public class Simplex
  {
    private double _Ftol = 1E-06;
    private int _Itmax = 500;
    private int _IterationDone;

    public double Ftol
    {
      get => this._Ftol;
      set => this._Ftol = value;
    }

    public int Itmax
    {
      get => this._Itmax;
      set => this._Itmax = value;
    }

    public int IterationDone => this._IterationDone;

    public void amoeba(IOBJfunc theFunction, double[,] par, double[] y, double[,] limits)
    {
      double num1 = 1E-20;
      double num2 = 1.0;
      double num3 = 0.5;
      double num4 = 2.0;
      int length = limits.GetLength(0);
      double[] Coefficient1 = new double[length];
      double[] Coefficient2 = new double[length];
      double[] numArray = new double[length];
      int num5 = length + 1;
      int num6 = 0;
      int num7 = 0;
      while (true)
      {
        ++num7;
        int index1 = 0;
        int index2;
        int index3;
        if (y[0] > y[1])
        {
          index2 = 0;
          index3 = 1;
        }
        else
        {
          index2 = 1;
          index3 = 0;
        }
        for (int index4 = 0; index4 < num5; ++index4)
        {
          if (y[index4] < y[index1])
            index1 = index4;
          if (y[index4] > y[index2])
          {
            index3 = index2;
            index2 = index4;
          }
          else if (y[index4] > y[index3] && index4 != index2)
            index3 = index4;
        }
        if (2.0 * Math.Abs(y[index2] - y[index1]) / (Math.Abs(y[index2]) + Math.Abs(y[index1]) + num1) >= this._Ftol)
        {
          if (num6 != this._Itmax)
          {
            ++num6;
            for (int index5 = 0; index5 < length; ++index5)
              numArray[index5] = 0.0;
            for (int index6 = 0; index6 < num5; ++index6)
            {
              if (index6 != index2)
              {
                for (int index7 = 0; index7 < length; ++index7)
                  numArray[index7] = numArray[index7] + par[index6, index7];
              }
            }
            for (int index8 = 0; index8 < length; ++index8)
            {
              numArray[index8] = numArray[index8] / (double) length;
              Coefficient1[index8] = (1.0 + num2) * numArray[index8] - num2 * par[index2, index8];
            }
            double num8 = theFunction.ObjfuncVal(Coefficient1, limits);
            if (num8 <= y[index1])
            {
              for (int index9 = 0; index9 < length; ++index9)
                Coefficient2[index9] = num4 * Coefficient1[index9] + (1.0 - num4) * numArray[index9];
              double num9 = theFunction.ObjfuncVal(Coefficient2, limits);
              if (num9 < y[index1])
              {
                for (int index10 = 0; index10 < length; ++index10)
                  par[index2, index10] = Coefficient2[index10];
                y[index2] = num9;
              }
              else
              {
                for (int index11 = 0; index11 < length; ++index11)
                  par[index2, index11] = Coefficient1[index11];
                y[index2] = num8;
              }
            }
            else if (num8 >= y[index3])
            {
              if (num8 < y[index2])
              {
                for (int index12 = 0; index12 < length; ++index12)
                  par[index2, index12] = Coefficient1[index12];
                y[index2] = num8;
              }
              for (int index13 = 0; index13 < length; ++index13)
                Coefficient2[index13] = num3 * par[index2, index13] + (1.0 - num3) * numArray[index13];
              double num10 = theFunction.ObjfuncVal(Coefficient2, limits);
              if (num10 < y[index2])
              {
                for (int index14 = 0; index14 < length; ++index14)
                  par[index2, index14] = Coefficient2[index14];
                y[index2] = num10;
              }
              else
              {
                for (int index15 = 0; index15 < num5; ++index15)
                {
                  if (index15 != index1)
                  {
                    for (int index16 = 0; index16 < length; ++index16)
                    {
                      Coefficient1[index16] = 0.5 * (par[index15, index16] + par[index1, index16]);
                      par[index15, index16] = Coefficient1[index16];
                    }
                    y[index15] = theFunction.ObjfuncVal(Coefficient1, limits);
                  }
                }
              }
            }
            else
            {
              for (int index17 = 0; index17 < length; ++index17)
                par[index2, index17] = Coefficient1[index17];
              y[index2] = num8;
            }
          }
          else
            goto label_17;
        }
        else
          break;
      }
      this._IterationDone = num6;
      return;
label_17:
      this._IterationDone = num6;
    }

    public void amoeba(IOBJfunc theFunction, double[,] par, double[,] limits, out double[] y)
    {
      int length = limits.GetLength(0);
      y = new double[length + 1];
      double[] numArray = new double[length + 1];
      double[] Coefficient = new double[length + 1];
      for (int index1 = 0; index1 <= length; ++index1)
      {
        for (int index2 = 0; index2 <= length - 1; ++index2)
          Coefficient[index2] = par[index1, index2];
        numArray[index1] = theFunction.ObjfuncVal(Coefficient, limits);
      }
      this.amoeba(theFunction, par, numArray, limits);
      Array.Copy((Array) numArray, (Array) y, length + 1);
    }
  }
}
