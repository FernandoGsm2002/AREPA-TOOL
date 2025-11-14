#!/usr/bin/env python3
"""
Samsung CSC Changer
Cambia el código CSC de dispositivos Samsung
"""

import serial
import time
import sys
import subprocess

def find_samsung_port():
    """Busca el puerto COM de Samsung"""
    try:
        # En Windows, buscar en puertos COM
        import serial.tools.list_ports
        ports = serial.tools.list_ports.comports()
        
        for port in ports:
            if 'SAMSUNG' in port.description.upper() or 'MODEM' in port.description.upper():
                print(f"[*] Puerto Samsung encontrado: {port.device} ({port.description})")
                return port.device
        
        print("[!] No se encontró puerto Samsung automáticamente")
        return None
    except:
        return None

def send_at_command(ser, command, wait=0.5):
    """Envía un comando AT y lee la respuesta"""
    try:
        # Limpiar buffers
        ser.reset_input_buffer()
        ser.reset_output_buffer()
        
        # Enviar comando
        print(f"  TX: {command.strip()}")
        ser.write((command + '\n').encode())
        time.sleep(wait)
        
        # Leer respuesta
        response = ""
        attempts = 0
        while attempts < 20:
            if ser.in_waiting > 0:
                response += ser.read(ser.in_waiting).decode('utf-8', errors='ignore')
            elif response:
                break
            time.sleep(0.1)
            attempts += 1
        
        if response:
            print(f"  RX: {response.strip()}")
        
        return response
    except Exception as e:
        print(f"  Error: {e}")
        return ""

def get_csc_list():
    """Obtiene la lista de CSC disponibles via ADB"""
    try:
        print("[*] Leyendo lista de CSC disponibles...")
        result = subprocess.run(
            ['adb', 'shell', 'cat', 'product/omc/sales_code_list.dat'],
            capture_output=True,
            text=True,
            timeout=10
        )
        
        if result.returncode == 0:
            csc_list = [line.strip() for line in result.stdout.split('\n') if line.strip()]
            # Filtrar solo códigos de 3 letras
            csc_list = [csc.replace('single/', '') for csc in csc_list]
            csc_list = [csc for csc in csc_list if len(csc) == 3 and csc.isalpha()]
            return csc_list
        return []
    except:
        return []

def change_csc(port_name, new_csc):
    """Cambia el CSC del dispositivo"""
    try:
        print(f"\n[*] Abriendo puerto {port_name}...")
        ser = serial.Serial(
            port=port_name,
            baudrate=115200,
            bytesize=serial.EIGHTBITS,
            parity=serial.PARITY_NONE,
            stopbits=serial.STOPBITS_ONE,
            timeout=5
        )
        
        print("[*] Puerto abierto correctamente\n")
        time.sleep(0.5)
        
        # Paso 1: Cambiar a modo DDEXE
        print("[1/6] Cambiando a modo DDEXE...")
        send_at_command(ser, "AT+SWATD=0")
        time.sleep(1)
        
        # Paso 2: Activar modo
        print("\n[2/6] Activando modo...")
        response = send_at_command(ser, "AT+ACTIVATE=0,0,0")
        if "OK" not in response:
            print("[!] ADVERTENCIA: Respuesta inesperada en activación")
        
        # Paso 3: Cambiar a modo ATD
        print("\n[3/6] Cambiando a modo ATD...")
        send_at_command(ser, "AT+SWATD=1")
        time.sleep(1)
        
        # Paso 4: Configurar nuevo CSC
        print(f"\n[4/6] Configurando CSC a {new_csc}...")
        response = send_at_command(ser, f"AT+PRECONFG=2,{new_csc}")
        if "OK" not in response:
            print("[✗] ERROR: No se pudo configurar el CSC")
            print(f"    Respuesta: {response}")
            ser.close()
            return False
        
        # Paso 5: Verificar configuración
        print("\n[5/6] Verificando configuración...")
        response = send_at_command(ser, "AT+PRECONFG=1,0")
        if new_csc in response:
            print(f"[✓] CSC verificado: {new_csc}")
        else:
            print(f"[!] ADVERTENCIA: CSC no coincide en verificación")
        
        # Paso 6: Reiniciar dispositivo
        print("\n[6/6] Reiniciando dispositivo...")
        send_at_command(ser, "AT+SWATD=0")
        time.sleep(0.5)
        send_at_command(ser, "AT+CFUN=1,1")
        
        print("\n" + "="*50)
        print("[✓] CSC CAMBIADO EXITOSAMENTE!")
        print(f"[✓] Nuevo CSC: {new_csc}")
        print("[*] El dispositivo se está reiniciando...")
        print("="*50)
        
        ser.close()
        return True
        
    except serial.SerialException as e:
        print(f"\n[✗] ERROR de puerto serial: {e}")
        print("    Verifica que el dispositivo esté en modo MTP")
        return False
    except Exception as e:
        print(f"\n[✗] ERROR: {e}")
        return False

def main():
    print("="*50)
    print("  SAMSUNG CSC CHANGER")
    print("="*50)
    print()
    
    # Verificar ADB
    try:
        subprocess.run(['adb', 'version'], capture_output=True, timeout=5)
    except:
        print("[✗] ERROR: ADB no encontrado")
        print("    Asegúrate de tener ADB en el PATH")
        sys.exit(1)
    
    # Obtener lista de CSC
    csc_list = get_csc_list()
    if csc_list:
        print("\n[*] CSC disponibles en tu dispositivo:")
        for i, csc in enumerate(csc_list, 1):
            print(f"    {i:2d}. {csc}")
        print()
    
    # Solicitar nuevo CSC
    new_csc = input("Ingresa el CSC que deseas (ej: ZTO, CHO, PEO): ").strip().upper()
    
    if len(new_csc) != 3:
        print("[✗] ERROR: El CSC debe tener 3 letras")
        sys.exit(1)
    
    if csc_list and new_csc not in csc_list:
        print(f"[!] ADVERTENCIA: {new_csc} no está en la lista de CSC disponibles")
        confirm = input("¿Deseas continuar de todos modos? (s/n): ")
        if confirm.lower() != 's':
            print("Operación cancelada")
            sys.exit(0)
    
    # Buscar puerto COM
    port = find_samsung_port()
    
    if not port:
        port = input("\nIngresa el puerto COM manualmente (ej: COM11): ").strip()
    
    if not port:
        print("[✗] ERROR: No se especificó puerto COM")
        sys.exit(1)
    
    # Confirmar
    print(f"\n[!] Se cambiará el CSC a: {new_csc}")
    print(f"[!] Usando puerto: {port}")
    confirm = input("\n¿Continuar? (s/n): ")
    
    if confirm.lower() != 's':
        print("Operación cancelada")
        sys.exit(0)
    
    # Cambiar CSC
    success = change_csc(port, new_csc)
    
    if success:
        print("\n[*] Proceso completado. El dispositivo se reiniciará.")
        sys.exit(0)
    else:
        print("\n[✗] El proceso falló. Revisa los errores arriba.")
        sys.exit(1)

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n[!] Operación cancelada por el usuario")
        sys.exit(1)
