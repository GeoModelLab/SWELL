// Decompiled with JetBrains decompiler
// Type: UNIMI.optimizer.SimpleGenetic
// Assembly: Optimizer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9A749CE1-E04E-4D6A-9AB7-E7251775AF42
// Assembly location: C:\Users\simoneugomaria.brega\Documents\gitProjects\swell\SWELL\runner\DLLs\Optimizer.dll

using System;
using System.Collections.Generic;
using System.Threading;

#nullable disable
namespace UNIMI.optimizer
{
  public class SimpleGenetic : IGenetic
  {
    private double _MutationRate = 0.0;
    private int _NofGeneration = 1;
    private double _SelectivePressure = 0.0;

    public void Calc(
      IOBJfunc ObjFunc,
      double[,] inputarray,
      int npar,
      double[,] limits,
      out double[,] outresults)
    {
      int index1 = 0;
      int index2 = 0;
      Thread.Sleep(1);
      RndUnif.RndUniform(-new Random().Next());
      int length1 = inputarray.GetLength(0);
      int length2 = inputarray.GetLength(1);
      double[,] numArray1 = new double[length1, npar + 2];
      double[,] numArray2 = new double[length1, length2];
      double[,] sourceArray = new double[length1, length2];
      for (int index3 = 0; index3 < length1; ++index3)
      {
        double num = RndUnif.RndUniform(4);
        if (((double) index3 + 0.5) / (double) length1 < num)
        {
          for (int index4 = 0; index4 < length2; ++index4)
            numArray2[index1, index4] = inputarray[index3, index4];
          ++index1;
        }
        else
        {
          for (int index5 = 0; index5 < length2; ++index5)
            sourceArray[index2, index5] = inputarray[index3, index5];
          ++index2;
        }
      }
      Array.Copy((Array) sourceArray, 0, (Array) numArray2, index1 * length2, index2 * length2);
      SimpleGenetic.Hibridation(npar, numArray2, numArray1);
      int length3 = (int) ((double) length1 * (1.0 - this._SelectivePressure));
      while (length3 % (npar + 1) != 0)
        ++length3;
      outresults = new double[length3, length2];
      Array.Copy((Array) numArray1, 0, (Array) outresults, 0, length3 * length2);
    }

    private static void Hibridation(int npar, double[,] tmpres, double[,] results)
    {
      List<int> intList1 = new List<int>();
      List<int> intList2 = new List<int>();
      List<int> intList3 = new List<int>();
      bool flag = false;
      int length1 = tmpres.GetLength(0);
      int length2 = tmpres.GetLength(1);
      int index1 = 0;
      for (int index2 = 0; index2 < length1; ++index2)
        intList1.Add(index2);
      while (intList1.Count > 0)
      {
        foreach (int index3 in intList1)
        {
          if (!intList3.Contains((int) tmpres[index3, npar + 1]))
          {
            intList2.Add(index3);
            intList3.Add((int) tmpres[index3, npar + 1]);
            if (intList2.Count == npar + 1)
            {
              flag = true;
              for (int index4 = 0; index4 < intList2.Count; ++index4)
              {
                for (int index5 = 0; index5 < length2; ++index5)
                  results[index1, index5] = tmpres[intList2[index4], index5];
                ++index1;
              }
            }
            if (flag)
            {
              flag = false;
              break;
            }
          }
        }
        for (int index6 = 0; index6 < intList2.Count; ++index6)
          intList1.Remove(intList2[index6]);
        intList2.Clear();
        intList3.Clear();
      }
    }

    public double MutationRate
    {
      get => this._MutationRate;
      set => this._MutationRate = value;
    }

    public int NofGeneration
    {
      get => this._NofGeneration;
      set => this._NofGeneration = value;
    }

    public double SelectivePressure
    {
      get => this._SelectivePressure;
      set => this._SelectivePressure = value;
    }
  }
}
