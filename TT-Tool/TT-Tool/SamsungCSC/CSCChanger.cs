using System;
using System.IO.Ports;
using System.Threading;

class CSCChanger
{
    static SerialPort port;
    
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Uso: CSCChanger.exe <PUERTO> <CSC>");
            Console.WriteLine("Ejemplo: CSCChanger.exe COM11 ZTO");
            Environment.Exit(1);
        }
        
        string portName = args[0];
        string newCSC = args[1].ToUpper();
        
        try
        {
            Console.WriteLine($"[*] Abriendo puerto {portName}...");
            port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
            port.ReadTimeout = 5000;
            port.WriteTimeout = 5000;
            port.Open();
            
            Console.WriteLine("[*] Puerto abierto correctamente");
            Thread.Sleep(500);
            
            // Paso 1: Cambiar a modo DDEXE
            Console.WriteLine("[1/6] Cambiando a modo DDEXE...");
            SendATCommand("AT+SWATD=0");
            Thread.Sleep(1000);
            
            // Paso 2: Activar modo
            Console.WriteLine("[2/6] Activando modo...");
            string response = SendATCommand("AT+ACTIVATE=0,0,0");
            if (!response.Contains("OK"))
            {
                Console.WriteLine("ERROR: No se pudo activar el modo");
                Environment.Exit(1);
            }
            
            // Paso 3: Cambiar a modo ATD
            Console.WriteLine("[3/6] Cambiando a modo ATD...");
            SendATCommand("AT+SWATD=1");
            Thread.Sleep(1000);
            
            // Paso 4: Configurar nuevo CSC
            Console.WriteLine($"[4/6] Configurando CSC a {newCSC}...");
            response = SendATCommand($"AT+PRECONFG=2,{newCSC}\r\n");
            if (!response.Contains("OK"))
            {
                Console.WriteLine("ERROR: No se pudo configurar el CSC");
                Console.WriteLine($"Respuesta: {response}");
                Environment.Exit(1);
            }
            
            // Paso 5: Verificar configuración
            Console.WriteLine("[5/6] Verificando configuración...");
            response = SendATCommand("AT+PRECONFG=1,0\r\n");
            Console.WriteLine($"CSC actual: {response}");
            
            if (!response.Contains(newCSC))
            {
                Console.WriteLine("ADVERTENCIA: El CSC no coincide");
            }
            
            // Paso 6: Reiniciar dispositivo
            Console.WriteLine("[6/6] Reiniciando dispositivo...");
            SendATCommand("AT+SWATD=0");
            Thread.Sleep(500);
            SendATCommand("AT+CFUN=1,1");
            
            Console.WriteLine("\n[✓] CSC cambiado exitosamente!");
            Console.WriteLine($"[✓] Nuevo CSC: {newCSC}");
            Console.WriteLine("[*] El dispositivo se esta reiniciando...");
            
            port.Close();
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERROR: {ex.Message}");
            if (port != null && port.IsOpen)
                port.Close();
            Environment.Exit(1);
        }
    }
    
    static string SendATCommand(string command)
    {
        try
        {
            // Limpiar buffer
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
            
            // Enviar comando
            Console.WriteLine($"  TX: {command.Trim()}");
            port.WriteLine(command);
            Thread.Sleep(500);
            
            // Leer respuesta
            string response = "";
            int attempts = 0;
            while (attempts < 10)
            {
                try
                {
                    if (port.BytesToRead > 0)
                    {
                        response += port.ReadExisting();
                    }
                    else if (response.Length > 0)
                    {
                        break;
                    }
                    Thread.Sleep(100);
                    attempts++;
                }
                catch
                {
                    break;
                }
            }
            
            if (response.Length > 0)
            {
                Console.WriteLine($"  RX: {response.Trim()}");
            }
            
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error: {ex.Message}");
            return "";
        }
    }
}
