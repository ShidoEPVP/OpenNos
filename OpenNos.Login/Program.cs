/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using log4net;
using OpenNos.Core;
using OpenNos.DAL.EF.MySQL;
using OpenNos.GameObject;
using OpenNos.Handler;
using OpenNos.ServiceRef.Internal;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace OpenNos.Login
{
    public class Program
    {
        #region Methods

        public static void Main()
        {
            checked
            {
                try
                {
                    //initialize Logger
                    Logger.InitializeLogger(LogManager.GetLogger(typeof(Program)));
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                    Console.Title = $"OpenNos Login Server v{fileVersionInfo.ProductVersion}";
                    Console.WriteLine("===============================================================================\n"
                                     + $"                 LOGIN SERVER VERSION {fileVersionInfo.ProductVersion} by OpenNos Team\n" +
                                     "===============================================================================\n");

                    //initialize DB
                    DataAccessHelper.Initialize();
                    Thread memory = new Thread(() => ServerManager.MemoryWatch("OpenNos Login Server"));
                    memory.Start();
                    string ip = System.Configuration.ConfigurationManager.AppSettings["LoginIp"];
                    int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LoginPort"]);
                    Logger.Log.Info(Language.Instance.GetMessageFromKey("CONFIG_LOADED"));
                    try
                    {
                        ServiceFactory.Instance.CommunicationService.Open();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error(ex.Message);
                    }
                    NetworkManager<LoginEncryption> networkManager = new NetworkManager<LoginEncryption>(ip, port, typeof(LoginPacketHandler));

                    //refresh WCF
                    ServiceFactory.Instance.CommunicationService.CleanupAsync();
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex.Message);
                    Console.ReadKey();
                }
            }
        }

        #endregion
    }
}