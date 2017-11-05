using SharpDX.DirectInput;
using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppForFights
{
    public partial class Form1 : Form
    {

        SerialPort currentPort;

        public Form1()
        {
            InitializeComponent();

            bool ArduinoPortFound = false;

            try
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    currentPort = new SerialPort(port, 9600);
                    if (ArduinoDetected())
                    {
                        ArduinoPortFound = true;
                        break;
                    }
                    else
                    {
                        ArduinoPortFound = false;
                    }
                }
            }
            catch { }

            if (ArduinoPortFound == false) return;
            System.Threading.Thread.Sleep(500);

            currentPort.BaudRate = 9600;
            currentPort.DtrEnable = true;
            currentPort.ReadTimeout = 1000;
            try
            {
                currentPort.Open();
            }
            catch { }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                    joystickGuid = deviceInstance.InstanceGuid;

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                lInfo.Text = "No joystick/Gamepad found.";
                Environment.Exit(1);
            }

            // Instantiate the joystick
            var joystick = new Joystick(directInput, joystickGuid);

            lInfo.Text = "Found Joystick/Gamepad with GUID:" + joystickGuid;

            // Query all suported ForceFeedback effects
            var allEffects = joystick.GetEffects();
            foreach (var effectInfo in allEffects)
            {
                label1.Text = "Effect available " + effectInfo.Name;
            }

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();

            // Poll events from joystick
            WaitForItToWork(joystick);
        }

        private bool ArduinoDetected()
        {
            try
            {
                currentPort.Open();
                System.Threading.Thread.Sleep(1000);
                string returnMessage = currentPort.ReadLine();
                currentPort.Close();

                if (returnMessage.Contains("I am Arduino"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private object locker = new object();

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            lock (locker)
            {
                if (currentPort.IsOpen) currentPort.Close();
            }
        }

        async void WaitForItToWork(Joystick joystick)
        {
            int jf = 0, jb = 0, jl = 0, jr = 0, bf = 0;

            while (true)
            {
                joystick.Poll();
                await Task.Delay(100);
                var states = joystick.GetCurrentState();
                if (states.Z / 128 < 256)
                {
                    jr = 0;
                    jl = -(states.Z / 128 - 255) > 30 ? -(states.Z / 128 - 255) : 0;
                    label2.Text = jl.ToString();
                    label1.Text = "Лево"; //Назад
                }
                else
                {
                    jl = 0;
                    jr = states.Z / 128 - 256 > 30 ? states.Z / 128 - 256 : 0;
                    label2.Text = jr.ToString();
                    label1.Text = "Право"; //вперед
                }

                if (states.Y / 128 < 256)
                {
                    jb = 0;
                    jf = -(states.Y / 128 - 255) > 30 ? -(states.Y / 128 - 255) : 0;
                    label3.Text = "Вперед"; //вправо
                    label4.Text = jf.ToString();
                }
                else
                {
                    jf = 0;
                    jb = states.Y / 128 - 256 > 30 ? states.Y / 128 - 256 : 0;
                    label4.Text = jb.ToString();
                    label3.Text = "Назад"; //влево
                }
                label6.Text = "Оружие";
                label5.Text = states.Buttons[6].ToString();
                if (states.Buttons[6] == false)
                    bf = 0;
                else
                    bf = 1;
                lock (locker)
                {
                    if (currentPort.IsOpen)
                        currentPort.Write(jr + " " + jl + " " + jb + " " + jf + " " + bf + "\n");
                }
            }
        }
    }
}
