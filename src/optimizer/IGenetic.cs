// Decompiled with JetBrains decompiler
// Type: UNIMI.optimizer.IGenetic
// Assembly: Optimizer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9A749CE1-E04E-4D6A-9AB7-E7251775AF42
// Assembly location: C:\Users\simoneugomaria.brega\Documents\gitProjects\swell\SWELL\runner\DLLs\Optimizer.dll

#nullable disable
namespace UNIMI.optimizer
{
  public interface IGenetic
  {
    void Calc(
      IOBJfunc ObjFunc,
      double[,] inputarray,
      int npar,
      double[,] limits,
      out double[,] results);

    double MutationRate { get; set; }

    int NofGeneration { get; set; }

    double SelectivePressure { get; set; }
  }
}
