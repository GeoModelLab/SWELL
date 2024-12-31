// Decompiled with JetBrains decompiler
// Type: UNIMI.optimizer.Sort2dArray
// Assembly: Sort2DArray, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C09F5E40-717B-4222-BD1C-E669AFEA58A1
// Assembly location: C:\Users\simoneugomaria.brega\Documents\gitProjects\swell\SWELL\runner\DLLs\Sort2DArray.dll

using Microsoft.VisualBasic;
using System;
using System.Diagnostics;


#nullable disable
namespace UNIMI.optimizer
{
  public class Sort2dArray
  {
    [DebuggerNonUserCode]
    public Sort2dArray()
    {
    }

    public static void Sort2d(
      ref double[,] arr,
      int firstEl,
      int lastEl,
      int ncols,
      int colToSort,
      bool descending)
    {
      double[] numArray = new double[checked (ncols + 1)];
      int num1 = checked (lastEl - firstEl + 1);
      int num2=0;
      do
      {
        num2 = num2 * 3 + 1;
      }
      while (num2 <= num1);
      do
      {

        num2 = checked ((int) Math.Round(unchecked ((double) num2 / 3.0)));
        int num3 = checked (num2 + firstEl);
        int num4 = lastEl;
        int index1 = num3;
        while (index1 <= num4)
        {
          double num5 = arr[index1, colToSort];
          int num6 = checked (ncols - 1);
          int index2 = 0;
          while (index2 <= num6)
          {
            numArray[index2] = arr[index1, index2];
            checked { ++index2; }
          }
          int index3 = index1;
          while (arr[checked (index3 - num2), colToSort] > num5 ^ descending)
          {
            int num7 = checked (ncols - 1);
            int index4 = 0;
            while (index4 <= num7)
            {
              arr[index3, index4] = arr[checked (index3 - num2), index4];
              checked { ++index4; }
            }
            checked { index3 -= num2; }
            if (checked (index3 - num2) < firstEl)
              break;
          }
          int num8 = checked (ncols - 1);
          int index5 = 0;
          while (index5 <= num8)
          {
            arr[index3, index5] = numArray[index5];
            checked { ++index5; }
          }
          checked { ++index1; }
        }
      }
      while (num2 != 1);
    }

    public static void Sort2d(ref double[,] arr, int colToSort, bool descending)
    {
      int firstEl = 0;
      int lastEl = Information.UBound((Array) arr);
      int ncols = checked (Information.UBound((Array) arr, 2) + 1);
      Sort2dArray.Sort2d(ref arr, firstEl, lastEl, ncols, colToSort, descending);
    }
  }
}
