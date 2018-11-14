using System;
using System.Diagnostics;
using System.IO;

/// *********************************************************************************************************
///  © 2014 www.jakemdrew.com All rights reserved. 
///  This source code is licensed under The GNU General Public License (GPLv3):  
///  http://opensource.org/licenses/gpl-3.0.html
/// *********************************************************************************************************

/// *********************************************************************************************************
/// RScriptRunner - Run R Programs From C#.
/// Created By - Jake Drew 
/// Version -    1.0, 06/23/2014
/// *********************************************************************************************************


/// This class runs R code from a file using the console.
/// 
/// If this code fails, it will typically fail "silently" without 
/// the R code running and without an error message.  This is normally 
/// due to an incorrect argument for the rScriptExecutablePath variable.
/// 

public class RScriptRunner
{

    /// Runs an R script from a file using Rscript.exe.
    /// 
    /// Example: 
    ///
    ///   RScriptRunner.RunFromCmd(curDirectory + @"\ImageClustering.r", "rscript.exe", curDirectory.Replace('\\','/'));
    ///   
    /// Getting args passed from C# using R:
    ///
    ///   args = commandArgs(trailingOnly = TRUE)
    ///   print(args[1]);
    /// 
    /// rCodeFilePath          - File where your R code is located.
    /// rScriptExecutablePath  - Usually only requires "rscript.exe"
    /// args                   - Multiple R args can be seperated by spaces.
    /// Returns                - a string with the R responses.
    public static string RunFromCmd(string rCodeFilePath, string rScriptExecutablePath, string args)
    {
        string file = rCodeFilePath;
        string result = string.Empty;

        try
        {

            var info = new ProcessStartInfo();
            info.FileName = rScriptExecutablePath;
            info.WorkingDirectory = Path.GetDirectoryName(rScriptExecutablePath);
            info.Arguments = rCodeFilePath + " " + args;

            info.RedirectStandardInput = false;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            using (var proc = new Process())
            {
                proc.StartInfo = info;
                proc.Start();
                result = proc.StandardOutput.ReadToEnd();
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception("R Script failed: " + result, ex);
        }
    }
}
