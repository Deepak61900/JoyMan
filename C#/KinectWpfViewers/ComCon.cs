using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace Microsoft.Samples.Kinect.WpfViewers
{
    class ComCon
    {
        public static bool isavailable = false;
        public static String buffer = "";
        static SerialPort _serialPort = new SerialPort();
        static Thread readThread = new Thread(Read);
        public ComCon()
        {
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = "COM40";
            _serialPort.BaudRate = 38400;
            _serialPort.Parity = _serialPort.Parity;
            _serialPort.DataBits = _serialPort.DataBits;
            _serialPort.StopBits = _serialPort.StopBits;
            _serialPort.Handshake = _serialPort.Handshake;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            
            try
            {
                _serialPort.Open();
            }
            catch
            {
            }
            readThread.Start();
        }
        public void writeStr(String message)
        {
            _serialPort.Write(message);
        }
        public String readStr()
        {
            String tmpp = buffer;
            buffer = "";
            isavailable = false;
            return tmpp;
        }
        public bool isAvailable()
        {
            return isavailable;
        }
        public static void Read()
        {
            while (true)
            {
                try
                {
                    buffer = buffer + _serialPort.ReadByte();
                    isavailable = true;
                }
                catch (TimeoutException) { }
            }
        }
    }
}
