// Decompiled with JetBrains decompiler
// Type: UNIMI.optimizer.MultiStartSimplex
// Assembly: Optimizer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9A749CE1-E04E-4D6A-9AB7-E7251775AF42
// Assembly location: C:\Users\simoneugomaria.brega\Documents\gitProjects\swell\SWELL\runner\DLLs\Optimizer.dll

using System;
using System.Threading;

#nullable disable
namespace UNIMI.optimizer
{
  public class MultiStartSimplex
  {
    private double _Ftol = 1E-07;
    private int _Itmax = 100;
    private int _IterationDone;
    private int _NofSimplexes = 100;

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

    public int NofSimplexes
    {
      get => this._NofSimplexes;
      set
      {
        this._NofSimplexes = value;
        if (this._NofSimplexes <= 10000)
          return;  
        this._NofSimplexes = 10000;
      }
    }

    public void Multistart(
      IOBJfunc theFunction,
      double[,] vertexarray,
      int nparam,
      double[,] limits,
      out double[,] results)
    {
      int length = vertexarray.GetLength(0);
      int num1 = 0;
      results = new double[length, nparam + 2];
      if (vertexarray.GetLength(0) != length)
      {
        //"incoherence between the number of row of the input vertex matrix and the number of parameters and simplexes", "Optimizer- Multiple start Simplex", MessageBoxButtons.OK);
      }
      else
      {
        Simplex simplex = new Simplex();
        simplex.Itmax = this._Itmax;
        simplex.Ftol = this._Ftol;
        double[,] par = new double[nparam + 1, nparam];
        for (int index1 = 0; index1 < length; index1 = index1 + nparam + 1)
        {
          for (int index2 = index1; index2 < index1 + nparam + 1; ++index2)
          {
            if (index2 == index1)
              ++num1;
            for (int index3 = 0; index3 < nparam; ++index3)
              par[index2 - index1, index3] = vertexarray[index2, index3];
          }
          double[] y;
          simplex.amoeba(theFunction, par, limits, out y);
          for (int index4 = index1; index4 < index1 + nparam + 1; ++index4)
          {
            for (int index5 = 0; index5 < nparam; ++index5)
              results[index4, index5] = par[index4 - index1, index5];
            results[index4, nparam] = y[index4 - index1];
            results[index4, nparam + 1] = (double) num1;
          }
        }
        Sort2dArray.Sort2d(ref results, nparam, false);
      }
    }

    public void Multistart(
      IOBJfunc theFunction,
      int nparam,
      double[,] limits,
      out double[,] results)
    {
      Thread.Sleep(1);
      double num = RndUnif.RndUniform(-new Random().Next());
      int length = (nparam + 1) * this._NofSimplexes;
      double[,] vertexarray = new double[length, nparam];
      for (int index1 = 0; index1 < length; ++index1)
      {
        for (int index2 = 0; index2 < nparam; ++index2)
          vertexarray[index1, index2] = RndUnif.RndUniform((int) (num * 1000.0), limits[index2, 0], limits[index2, 1]);
      }
      this.Multistart(theFunction, vertexarray, nparam, limits, out results);
    }
  }
}
