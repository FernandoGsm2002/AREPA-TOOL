using System.Diagnostics;
using System.IO.Ports;
using System.Text;

namespace TT_Tool.Managers
{
    /// <summary>
    /// Manager para operaciones QCN (Qualcomm Calibration Network)
    /// Permite leer, escribir y editar archivos QCN para reparación de IMEI
    /// </summary>
    public class QCNManager
    {
        private LogManager? _logManager;
        private SerialPort? _diagPort;
        
        // Comandos QCDM (Qualcomm Diagnostic Monitor)
        private static readonly byte[] CMD_VERSION_INFO = { 0x00 };
        private static readonly byte[] CMD_ESN = { 0x01 };
        private static readonly byte[] CMD_DIAG_VER = { 0x3C };
        private static readonly byte[] CMD_STATUS = { 0x0C };
        private static readonly byte[] CMD_NV_READ = { 0x26 };
        private static readonly byte[] CMD_NV_WRITE = { 0x27 };
        
        // NV Items para IMEI
        private const ushort NV_ITEM_IMEI = 550;  // NV_UE_IMEI_I
        private const ushort NV_ITEM_MEID = 1943; // NV_MEID_I
        
        public QCNManager()
        {
        }
        
        public void SetLogManager(LogManager logManager)
        {
            _logManager = logManager;
        }
        
        private void Log(string mensaje, TipoLog tipo = TipoLog.Info)
        {
            _logManager?.AgregarLog(mensaje, tipo);
        }
        
        /// <summary>
        /// Detecta puertos DIAG disponibles
        /// </summary>
        public List<string> DetectarPuertosDiag()
        {
            var puertosDiag = new List<string>();
            
            try
            {
                var puertos = SerialPort.GetPortNames();
                
                foreach (var puerto in puertos)
                {
                    try
                    {
                        using var port = new SerialPort(puerto, 115200, Parity.None, 8, StopBits.One);
                        port.ReadTimeout = 1000;
                        port.WriteTimeout = 1000;
                        
                        port.Open();
                        
                        // Enviar comando de versión para verificar si es DIAG
                        byte[] cmd = PrepararComandoDiag(CMD_VERSION_INFO);
                        port.Write(cmd, 0, cmd.Length);
                        
                        Thread.Sleep(100);
                        
                        if (port.BytesToRead > 0)
                        {
                            puertosDiag.Add(puerto);
                        }
                        
                        port.Close();
                    }
                    catch
                    {
                        // Puerto no es DIAG o no está disponible
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error detectando puertos DIAG: {ex.Message}", TipoLog.Error);
            }
            
            return puertosDiag;
        }
        
        /// <summary>
        /// Conecta al puerto DIAG
        /// </summary>
        public bool ConectarPuertoDiag(string puerto)
        {
            try
            {
                if (_diagPort != null && _diagPort.IsOpen)
                {
                    _diagPort.Close();
                }
                
                _diagPort = new SerialPort(puerto, 115200, Parity.None, 8, StopBits.One)
                {
                    ReadTimeout = 5000,
                    WriteTimeout = 5000,
                    Handshake = Handshake.None,
                    DtrEnable = true,
                    RtsEnable = true
                };
                
                _diagPort.Open();
                
                // Verificar conexión
                byte[] cmd = PrepararComandoDiag(CMD_VERSION_INFO);
                _diagPort.Write(cmd, 0, cmd.Length);
                
                Thread.Sleep(200);
                
                if (_diagPort.BytesToRead > 0)
                {
                    Log($"Conectado a puerto DIAG: {puerto}", TipoLog.Exito);
                    return true;
                }
                
                _diagPort.Close();
                return false;
            }
            catch (Exception ex)
            {
                Log($"Error conectando a puerto DIAG: {ex.Message}", TipoLog.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Lee archivo QCN del dispositivo
        /// </summary>
        public async Task<bool> LeerQCN(string rutaSalida)
        {
            try
            {
                if (_diagPort == null || !_diagPort.IsOpen)
                {
                    Log("Puerto DIAG no conectado", TipoLog.Error);
                    return false;
                }
                
                Log("=== LEYENDO QCN ===");
                Log($"Archivo de salida: {rutaSalida}");
                Log("");
                
                // Leer información del dispositivo
                Log("Leyendo información del dispositivo...");
                var deviceInfo = await LeerInformacionDispositivo();
                
                if (deviceInfo == null)
                {
                    Log("No se pudo leer información del dispositivo", TipoLog.Error);
                    return false;
                }
                
                Log($"Modelo: {deviceInfo.Model}");
                Log($"IMEI 1: {deviceInfo.IMEI1}");
                Log($"IMEI 2: {deviceInfo.IMEI2}");
                Log($"Tipo: {(deviceInfo.IsDualSIM ? "Dual SIM" : "Single SIM")}");
                Log("");
                
                // Crear estructura QCN
                Log("Creando archivo QCN...");
                var qcnData = await CrearArchivoQCN(deviceInfo);
                
                if (qcnData == null || qcnData.Length == 0)
                {
                    Log("Error creando archivo QCN", TipoLog.Error);
                    return false;
                }
                
                // Guardar archivo
                await File.WriteAllBytesAsync(rutaSalida, qcnData);
                
                Log($"QCN guardado: {rutaSalida}", TipoLog.Exito);
                Log($"Tamaño: {qcnData.Length / 1024} KB");
                Log("");
                Log("✓ Lectura completada exitosamente", TipoLog.Exito);
                
                return true;
            }
            catch (Exception ex)
            {
                Log($"Error leyendo QCN: {ex.Message}", TipoLog.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Escribe archivo QCN al dispositivo
        /// </summary>
        public async Task<bool> EscribirQCN(string rutaQCN)
        {
            try
            {
                if (_diagPort == null || !_diagPort.IsOpen)
                {
                    Log("Puerto DIAG no conectado", TipoLog.Error);
                    return false;
                }
                
                if (!File.Exists(rutaQCN))
                {
                    Log("Archivo QCN no encontrado", TipoLog.Error);
                    return false;
                }
                
                Log("=== ESCRIBIENDO QCN ===");
                Log($"Archivo: {Path.GetFileName(rutaQCN)}");
                Log("");
                
                // Leer archivo QCN
                var qcnData = await File.ReadAllBytesAsync(rutaQCN);
                Log($"Tamaño del archivo: {qcnData.Length / 1024} KB");
                
                // Parsear QCN
                var deviceInfo = ParsearArchivoQCN(qcnData);
                
                if (deviceInfo == null)
                {
                    Log("Archivo QCN inválido", TipoLog.Error);
                    return false;
                }
                
                Log($"IMEI 1: {deviceInfo.IMEI1}");
                Log($"IMEI 2: {deviceInfo.IMEI2}");
                Log($"Tipo: {(deviceInfo.IsDualSIM ? "Dual SIM" : "Single SIM")}");
                Log("");
                
                // Escribir al dispositivo
                Log("Escribiendo datos al dispositivo...");
                bool resultado = await EscribirDatosDispositivo(deviceInfo);
                
                if (resultado)
                {
                    Log("");
                    Log("✓ QCN escrito exitosamente", TipoLog.Exito);
                    Log("Reinicia el dispositivo para aplicar cambios");
                    return true;
                }
                else
                {
                    Log("Error escribiendo QCN", TipoLog.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log($"Error escribiendo QCN: {ex.Message}", TipoLog.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Edita IMEI en archivo QCN
        /// </summary>
        public async Task<bool> EditarIMEIEnQCN(string rutaQCN, string nuevoIMEI1, string? nuevoIMEI2 = null)
        {
            try
            {
                if (!File.Exists(rutaQCN))
                {
                    Log("Archivo QCN no encontrado", TipoLog.Error);
                    return false;
                }
                
                Log("=== EDITANDO QCN ===");
                Log($"Archivo: {Path.GetFileName(rutaQCN)}");
                Log("");
                
                // Validar IMEIs
                if (!ValidarIMEI(nuevoIMEI1))
                {
                    Log("IMEI 1 inválido (debe ser 15 dígitos)", TipoLog.Error);
                    return false;
                }
                
                if (nuevoIMEI2 != null && !ValidarIMEI(nuevoIMEI2))
                {
                    Log("IMEI 2 inválido (debe ser 15 dígitos)", TipoLog.Error);
                    return false;
                }
                
                // Leer archivo
                var qcnData = await File.ReadAllBytesAsync(rutaQCN);
                var deviceInfo = ParsearArchivoQCN(qcnData);
                
                if (deviceInfo == null)
                {
                    Log("Archivo QCN inválido", TipoLog.Error);
                    return false;
                }
                
                Log($"IMEI 1 actual: {deviceInfo.IMEI1}");
                Log($"IMEI 1 nuevo:  {nuevoIMEI1}");
                
                if (nuevoIMEI2 != null)
                {
                    Log($"IMEI 2 actual: {deviceInfo.IMEI2}");
                    Log($"IMEI 2 nuevo:  {nuevoIMEI2}");
                }
                
                Log("");
                
                // Actualizar IMEIs
                deviceInfo.IMEI1 = nuevoIMEI1;
                if (nuevoIMEI2 != null)
                {
                    deviceInfo.IMEI2 = nuevoIMEI2;
                    deviceInfo.IsDualSIM = true;
                }
                
                // Crear nuevo QCN
                var nuevoQCN = await CrearArchivoQCN(deviceInfo);
                
                if (nuevoQCN == null)
                {
                    Log("Error creando QCN modificado", TipoLog.Error);
                    return false;
                }
                
                // Guardar backup
                string backup = rutaQCN + ".backup";
                File.Copy(rutaQCN, backup, true);
                Log($"Backup creado: {Path.GetFileName(backup)}");
                
                // Guardar nuevo QCN
                await File.WriteAllBytesAsync(rutaQCN, nuevoQCN);
                
                Log("");
                Log("✓ QCN editado exitosamente", TipoLog.Exito);
                Log("Puedes escribir este QCN al dispositivo");
                
                return true;
            }
            catch (Exception ex)
            {
                Log($"Error editando QCN: {ex.Message}", TipoLog.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Lee información del dispositivo vía DIAG
        /// </summary>
        private async Task<DeviceInfo?> LeerInformacionDispositivo()
        {
            try
            {
                var info = new DeviceInfo();
                
                // Leer IMEI 1
                var imei1Data = await LeerNVItem(NV_ITEM_IMEI);
                if (imei1Data != null && imei1Data.Length >= 9)
                {
                    info.IMEI1 = ConvertirBytesAIMEI(imei1Data);
                }
                
                // Intentar leer IMEI 2 (para dual SIM)
                var imei2Data = await LeerNVItem((ushort)(NV_ITEM_IMEI + 1));
                if (imei2Data != null && imei2Data.Length >= 9)
                {
                    info.IMEI2 = ConvertirBytesAIMEI(imei2Data);
                    info.IsDualSIM = !string.IsNullOrEmpty(info.IMEI2) && info.IMEI2 != "000000000000000";
                }
                
                // Leer modelo
                info.Model = "Qualcomm Device";
                
                return info;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Lee un NV Item del dispositivo
        /// </summary>
        private async Task<byte[]?> LeerNVItem(ushort itemId)
        {
            try
            {
                if (_diagPort == null || !_diagPort.IsOpen)
                    return null;
                
                // Construir comando NV_READ
                var cmd = new List<byte> { CMD_NV_READ[0] };
                cmd.AddRange(BitConverter.GetBytes(itemId));
                
                byte[] cmdData = PrepararComandoDiag(cmd.ToArray());
                
                _diagPort.DiscardInBuffer();
                _diagPort.Write(cmdData, 0, cmdData.Length);
                
                await Task.Delay(200);
                
                if (_diagPort.BytesToRead > 0)
                {
                    byte[] response = new byte[_diagPort.BytesToRead];
                    _diagPort.Read(response, 0, response.Length);
                    
                    // Decodificar respuesta HDLC
                    var decoded = DecodificarHDLC(response);
                    
                    if (decoded != null && decoded.Length > 4)
                    {
                        // Extraer datos (después del header)
                        byte[] data = new byte[decoded.Length - 4];
                        Array.Copy(decoded, 4, data, 0, data.Length);
                        return data;
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Escribe datos al dispositivo
        /// </summary>
        private async Task<bool> EscribirDatosDispositivo(DeviceInfo deviceInfo)
        {
            try
            {
                // Escribir IMEI 1
                var imei1Bytes = ConvertirIMEIABytes(deviceInfo.IMEI1);
                bool resultado1 = await EscribirNVItem(NV_ITEM_IMEI, imei1Bytes);
                
                if (!resultado1)
                {
                    Log("Error escribiendo IMEI 1", TipoLog.Error);
                    return false;
                }
                
                Log("IMEI 1 escrito correctamente", TipoLog.Exito);
                
                // Escribir IMEI 2 si es dual SIM
                if (deviceInfo.IsDualSIM && !string.IsNullOrEmpty(deviceInfo.IMEI2))
                {
                    var imei2Bytes = ConvertirIMEIABytes(deviceInfo.IMEI2);
                    bool resultado2 = await EscribirNVItem((ushort)(NV_ITEM_IMEI + 1), imei2Bytes);
                    
                    if (!resultado2)
                    {
                        Log("Error escribiendo IMEI 2", TipoLog.Error);
                        return false;
                    }
                    
                    Log("IMEI 2 escrito correctamente", TipoLog.Exito);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Log($"Error escribiendo datos: {ex.Message}", TipoLog.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Escribe un NV Item al dispositivo
        /// </summary>
        private async Task<bool> EscribirNVItem(ushort itemId, byte[] data)
        {
            try
            {
                if (_diagPort == null || !_diagPort.IsOpen)
                    return false;
                
                // Construir comando NV_WRITE
                var cmd = new List<byte> { CMD_NV_WRITE[0] };
                cmd.AddRange(BitConverter.GetBytes(itemId));
                cmd.AddRange(data);
                
                byte[] cmdData = PrepararComandoDiag(cmd.ToArray());
                
                _diagPort.DiscardInBuffer();
                _diagPort.Write(cmdData, 0, cmdData.Length);
                
                await Task.Delay(300);
                
                if (_diagPort.BytesToRead > 0)
                {
                    byte[] response = new byte[_diagPort.BytesToRead];
                    _diagPort.Read(response, 0, response.Length);
                    
                    // Verificar respuesta exitosa
                    return response.Length > 0 && response[0] == CMD_NV_WRITE[0];
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Crea archivo QCN desde información del dispositivo
        /// </summary>
        private async Task<byte[]?> CrearArchivoQCN(DeviceInfo deviceInfo)
        {
            try
            {
                using var ms = new MemoryStream();
                using var writer = new BinaryWriter(ms);
                
                // Header QCN
                writer.Write(Encoding.ASCII.GetBytes("QCNV"));  // Magic
                writer.Write((ushort)1);                         // Version
                writer.Write((uint)0);                           // Reserved
                
                // IMEI 1
                writer.Write((ushort)NV_ITEM_IMEI);
                var imei1Bytes = ConvertirIMEIABytes(deviceInfo.IMEI1);
                writer.Write((ushort)imei1Bytes.Length);
                writer.Write(imei1Bytes);
                
                // IMEI 2 (si es dual SIM)
                if (deviceInfo.IsDualSIM && !string.IsNullOrEmpty(deviceInfo.IMEI2))
                {
                    writer.Write((ushort)(NV_ITEM_IMEI + 1));
                    var imei2Bytes = ConvertirIMEIABytes(deviceInfo.IMEI2);
                    writer.Write((ushort)imei2Bytes.Length);
                    writer.Write(imei2Bytes);
                }
                
                // Footer
                writer.Write((ushort)0xFFFF);  // End marker
                
                return await Task.FromResult(ms.ToArray());
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Parsea archivo QCN
        /// </summary>
        private DeviceInfo? ParsearArchivoQCN(byte[] qcnData)
        {
            try
            {
                var info = new DeviceInfo();
                
                // Intentar formato estándar QCNV primero
                if (qcnData.Length >= 4)
                {
                    var magic = Encoding.ASCII.GetString(qcnData, 0, 4);
                    if (magic == "QCNV")
                    {
                        return ParsearQCNEstandar(qcnData);
                    }
                }
                
                // Si no es formato estándar, buscar IMEI en hexadecimal
                var imeisEncontrados = BuscarIMEIEnHex(qcnData);
                
                if (imeisEncontrados.Count > 0)
                {
                    // Invertir orden: el primero encontrado es IMEI 2, el segundo es IMEI 1
                    if (imeisEncontrados.Count > 1)
                    {
                        info.IMEI1 = imeisEncontrados[1];
                        info.IMEI2 = imeisEncontrados[0];
                        info.IsDualSIM = true;
                    }
                    else
                    {
                        info.IMEI1 = imeisEncontrados[0];
                    }
                    
                    return info;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Log($"Error parseando QCN: {ex.Message}", TipoLog.Error);
                return null;
            }
        }
        
        /// <summary>
        /// Parsea archivo QCN en formato estándar
        /// </summary>
        private DeviceInfo? ParsearQCNEstandar(byte[] qcnData)
        {
            try
            {
                using var ms = new MemoryStream(qcnData);
                using var reader = new BinaryReader(ms);
                
                // Leer header
                var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
                var version = reader.ReadUInt16();
                var reserved = reader.ReadUInt32();
                
                var info = new DeviceInfo();
                
                // Leer items
                while (ms.Position < ms.Length - 2)
                {
                    var itemId = reader.ReadUInt16();
                    
                    if (itemId == 0xFFFF)  // End marker
                        break;
                    
                    var dataLength = reader.ReadUInt16();
                    var data = reader.ReadBytes(dataLength);
                    
                    if (itemId == NV_ITEM_IMEI)
                    {
                        info.IMEI1 = ConvertirBytesAIMEI(data);
                    }
                    else if (itemId == NV_ITEM_IMEI + 1)
                    {
                        info.IMEI2 = ConvertirBytesAIMEI(data);
                        info.IsDualSIM = true;
                    }
                }
                
                return info;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Busca IMEIs en formato hexadecimal en el archivo
        /// </summary>
        private List<string> BuscarIMEIEnHex(byte[] data)
        {
            var imeisEncontrados = new List<string>();
            
            for (int i = 0; i < data.Length - 9; i++)
            {
                if (data[i] == 0x08)
                {
                    var imei = ExtraerIMEIDesdeBytes(data, i);
                    
                    if (!string.IsNullOrEmpty(imei) && EsIMEIValido(imei))
                    {
                        if (!imeisEncontrados.Contains(imei))
                        {
                            imeisEncontrados.Add(imei);
                        }
                    }
                }
            }
            
            return imeisEncontrados;
        }
        
        /// <summary>
        /// Valida si un IMEI es válido usando checksum Luhn y prefijos conocidos
        /// </summary>
        private bool EsIMEIValido(string imei)
        {
            // Verificar longitud
            if (string.IsNullOrEmpty(imei) || imei.Length != 15)
                return false;
            
            // Verificar que todos sean dígitos
            if (!imei.All(char.IsDigit))
                return false;
            
            // Los IMEIs válidos deben empezar con ciertos dígitos (TAC - Type Allocation Code)
            // Típicamente: 01, 10, 30, 33, 35, 44, 45, 49, 50, 51, 52, 53, 54, 86, 87, 88, 89, 90, 91, 99
            string prefix = imei.Substring(0, 2);
            var validPrefixes = new[] { "01", "10", "30", "33", "35", "44", "45", "49", "50", "51", "52", "53", "54", "86", "87", "88", "89", "90", "91", "99" };
            
            if (!validPrefixes.Contains(prefix))
                return false;
            
            // Validar checksum Luhn
            return ValidarLuhnChecksum(imei);
        }
        
        /// <summary>
        /// Valida el checksum Luhn del IMEI
        /// </summary>
        private bool ValidarLuhnChecksum(string imei)
        {
            try
            {
                int sum = 0;
                bool alternate = false;
                
                // Procesar de derecha a izquierda
                for (int i = imei.Length - 1; i >= 0; i--)
                {
                    int digit = int.Parse(imei[i].ToString());
                    
                    if (alternate)
                    {
                        digit *= 2;
                        if (digit > 9)
                            digit -= 9;
                    }
                    
                    sum += digit;
                    alternate = !alternate;
                }
                
                return (sum % 10) == 0;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Extrae IMEI desde una posición específica en formato BCD invertido
        /// </summary>
        private string ExtraerIMEIDesdeBytes(byte[] data, int offset)
        {
            try
            {
                if (offset + 9 > data.Length)
                    return "";
                
                var imei = new StringBuilder();
                int startByte = offset + 1;
                
                // Leer 8 bytes en formato BCD invertido
                for (int i = 0; i < 8; i++)
                {
                    byte b = data[startByte + i];
                    int lowNibble = b & 0x0F;
                    int highNibble = (b >> 4) & 0x0F;
                    
                    if (lowNibble <= 9 && imei.Length < 15)
                        imei.Append(lowNibble);
                    
                    if (highNibble <= 9 && imei.Length < 15)
                        imei.Append(highNibble);
                    
                    if (imei.Length >= 15)
                        break;
                }
                
                return imei.Length == 15 ? imei.ToString() : "";
            }
            catch
            {
                return "";
            }
        }
        
        /// <summary>
        /// Prepara comando DIAG con encoding HDLC
        /// </summary>
        private byte[] PrepararComandoDiag(byte[] comando)
        {
            // Calcular CRC
            ushort crc = CalcularCRC(comando);
            
            var conCRC = new List<byte>(comando);
            conCRC.AddRange(BitConverter.GetBytes(crc));
            
            // Encoding HDLC
            return CodificarHDLC(conCRC.ToArray());
        }
        
        /// <summary>
        /// Codifica datos en formato HDLC
        /// </summary>
        private byte[] CodificarHDLC(byte[] data)
        {
            var encoded = new List<byte> { 0x7E };  // Flag byte
            
            foreach (byte b in data)
            {
                if (b == 0x7E || b == 0x7D)
                {
                    encoded.Add(0x7D);  // Escape byte
                    encoded.Add((byte)(b ^ 0x20));
                }
                else
                {
                    encoded.Add(b);
                }
            }
            
            encoded.Add(0x7E);  // Flag byte
            return encoded.ToArray();
        }
        
        /// <summary>
        /// Decodifica datos en formato HDLC
        /// </summary>
        private byte[]? DecodificarHDLC(byte[] data)
        {
            var decoded = new List<byte>();
            bool escape = false;
            
            for (int i = 1; i < data.Length - 1; i++)  // Skip flag bytes
            {
                if (data[i] == 0x7D)
                {
                    escape = true;
                    continue;
                }
                
                if (escape)
                {
                    decoded.Add((byte)(data[i] ^ 0x20));
                    escape = false;
                }
                else if (data[i] != 0x7E)
                {
                    decoded.Add(data[i]);
                }
            }
            
            return decoded.Count > 0 ? decoded.ToArray() : null;
        }
        
        /// <summary>
        /// Calcula CRC-16-CCITT
        /// </summary>
        private ushort CalcularCRC(byte[] data)
        {
            ushort crc = 0xFFFF;
            
            foreach (byte b in data)
            {
                crc ^= (ushort)(b << 8);
                
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }
            
            return (ushort)~crc;
        }
        
        /// <summary>
        /// Convierte bytes a IMEI string en formato BCD invertido
        /// </summary>
        private string ConvertirBytesAIMEI(byte[] data)
        {
            if (data.Length < 8)
                return "000000000000000";
            
            var imei = new StringBuilder();
            
            // Determinar offset inicial (algunos formatos tienen byte de longitud, otros no)
            int startOffset = (data.Length >= 9 && data[0] == 0x08) ? 1 : 0;
            
            // Leer en formato BCD invertido
            for (int i = startOffset; i < Math.Min(startOffset + 8, data.Length); i++)
            {
                int lowNibble = data[i] & 0x0F;        // Primer dígito
                int highNibble = (data[i] >> 4) & 0x0F; // Segundo dígito
                
                // En BCD invertido, el nibble bajo va primero
                if (lowNibble <= 9 && imei.Length < 15)
                    imei.Append(lowNibble);
                
                if (highNibble <= 9 && imei.Length < 15)
                    imei.Append(highNibble);
                
                if (imei.Length >= 15)
                    break;
            }
            
            // Rellenar con ceros si es necesario
            while (imei.Length < 15)
                imei.Append('0');
            
            return imei.ToString().Substring(0, 15);
        }
        
        /// <summary>
        /// Convierte IMEI string a bytes
        /// </summary>
        private byte[] ConvertirIMEIABytes(string imei)
        {
            if (imei.Length != 15)
                throw new ArgumentException("IMEI debe tener 15 dígitos");
            
            var bytes = new byte[9];
            bytes[0] = 0x08;  // Length
            
            for (int i = 0; i < 7; i++)
            {
                int digit1 = int.Parse(imei[i * 2].ToString());
                int digit2 = int.Parse(imei[i * 2 + 1].ToString());
                bytes[i + 1] = (byte)((digit2 << 4) | digit1);
            }
            
            // Último dígito
            int lastDigit = int.Parse(imei[14].ToString());
            bytes[8] = (byte)(0xF0 | lastDigit);
            
            return bytes;
        }
        
        /// <summary>
        /// Valida formato de IMEI
        /// </summary>
        private bool ValidarIMEI(string imei)
        {
            if (string.IsNullOrEmpty(imei) || imei.Length != 15)
                return false;
            
            return imei.All(char.IsDigit);
        }
        
        /// <summary>
        /// Cierra conexión DIAG
        /// </summary>
        public void Desconectar()
        {
            try
            {
                if (_diagPort != null && _diagPort.IsOpen)
                {
                    _diagPort.Close();
                    _diagPort.Dispose();
                    _diagPort = null;
                }
            }
            catch
            {
                // Ignorar errores al cerrar
            }
        }
        
        /// <summary>
        /// Clase para almacenar información del dispositivo
        /// </summary>
        public class DeviceInfo
        {
            public string Model { get; set; } = "";
            public string IMEI1 { get; set; } = "";
            public string IMEI2 { get; set; } = "";
            public bool IsDualSIM { get; set; } = false;
        }
    }
}
