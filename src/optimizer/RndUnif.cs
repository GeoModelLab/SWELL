// Decompiled with JetBrains decompiler
// Type: UNIMI.optimizer.RndUnif
// Assembly: Optimizer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9A749CE1-E04E-4D6A-9AB7-E7251775AF42
// Assembly location: C:\Users\simoneugomaria.brega\Documents\gitProjects\swell\SWELL\runner\DLLs\Optimizer.dll

#nullable disable
namespace UNIMI.optimizer
{
  public class RndUnif
  {
    private static int Iffx;
    private static double[] R = new double[97];
    private static int Ix1 = 0;
    private static int Ix2 = 0;
    private static int Ix3 = 0;

    public static double RndUniform(int idum)
    {
      int num1 = 259200;
      int num2 = 134456;
      int num3 = 243000;
      double num4 = 3.8580247E-06;
      double num5 = 7.4373773E-06;
      int num6 = 7141;
      int num7 = 8121;
      int num8 = 4561;
      int num9 = 54773;
      int num10 = 28411;
      int num11 = 51349;
      if (idum < 0 | RndUnif.Iffx == 0)
      {
        RndUnif.Iffx = 1;
        RndUnif.Ix1 = (num9 - idum) % num1;
        RndUnif.Ix1 = (num6 * RndUnif.Ix1 + num9) % num1;
        RndUnif.Ix2 = RndUnif.Ix1 % num2;
        RndUnif.Ix1 = (num6 * RndUnif.Ix1 + num9) % num1;
        RndUnif.Ix3 = RndUnif.Ix1 % num3;
        for (int index = 0; index <= 96; ++index)
        {
          RndUnif.Ix1 = (num6 * RndUnif.Ix1 + num9) % num1;
          RndUnif.Ix2 = (num7 * RndUnif.Ix2 + num10) % num2;
          RndUnif.R[index] = ((double) RndUnif.Ix1 + (double) RndUnif.Ix2 * num5) * num4;
        }
        idum = 1;
      }
      RndUnif.Ix1 = (num6 * RndUnif.Ix1 + num9) % num1;
      RndUnif.Ix2 = (num7 * RndUnif.Ix2 + num10) % num2;
      RndUnif.Ix3 = (num8 * RndUnif.Ix3 + num11) % num3;
      int index1 = 96 * RndUnif.Ix3 / num3;
      RndUnif.R[index1] = ((double) RndUnif.Ix1 + (double) RndUnif.Ix2 * num5) * num4;
      return RndUnif.R[index1];
    }

    public static double RndUniform(int idum, double n1, double n2)
    {
      return n1 + (n2 - n1) * RndUnif.RndUniform(idum);
    }
  }
}
