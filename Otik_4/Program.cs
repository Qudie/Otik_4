using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Remoting;

namespace Otik_4
{
    public class Tree
    {
        internal string frec;
        internal string letra;
        internal Tree hijoIzq;
        internal Tree hijoDer;

        public Tree()
        {
            hijoIzq = null;
            hijoDer = null;
        }


        public virtual void colocaIzq(Tree V, Tree t)
        {
            t.hijoIzq = V;
        }

        public virtual void colocaDer(Tree V, Tree t)
        {
            t.hijoDer = V;
        }

        public virtual void preOrden(Tree A)
        {

            if (A != null)
            {
                Console.WriteLine(A.letra + "	" + A.frec + " ");
                preOrden(A.hijoIzq);
                preOrden(A.hijoDer);
            }
        }

    }

    public class Program
    {
        static int header_length = 32;

        static byte[] MakeHeaderNew(int count_files, int source_length, int result_length)
        {
            /*
            char[] otik_sig = new char[] { 'O', 'T', 'I', 'K' };
            char[] lab = new char[] { '4' };
            char[] v = new char[] { '1' };
            char[] cypher = new char[] { (char)0x1 };  // 0x1 - Huffman, 0x2 - ..., 0x4 - ..., 0x8 - ...
            char[] res = new char[] { '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0' };
            char[] src_len = new char[] { (char)(source_length >> 24), (char)(source_length >> 16), (char)(source_length >> 8), (char)(source_length) };
            char[] res_len = new char[] { (char)(result_length >> 24), (char)(result_length >> 16), (char)(result_length >> 8), (char)(result_length) };
            */
            byte[] hdr = new byte[] {
                (int)'O', (int)'T', (int)'I', (int)'K',
                4,
                1,
                0x1,
                (byte)(count_files >> 24), (byte)(count_files >> 16), (byte)(count_files >> 8), (byte)(count_files),
                0, 0, 0, 0, 0,
                (byte)(source_length >> 24), (byte)(source_length >> 16), (byte)(source_length >> 8), (byte)(source_length),
                (byte)(result_length >> 24), (byte)(result_length >> 16), (byte)(result_length >> 8), (byte)(result_length)
            };
            Console.WriteLine("Header len = " + hdr.Length);
            return hdr;
        }

        static string MakeHeader(int count_files, int source_length, int result_length)
        {
            int cipher = 1;
            char reserved = '\0';
            string header = "OTIK"; //0-3 сигнатура
            header += "4";//4 номер лабы
            header += "v1"; //5-6 версия
            header += "Huffman"; //7 шифр применяемых типов кодирования
            header += "3"; //8 кол-во файлов в архиве
            //9-15 зарезервировано
            for (int i = 0; i < 7; i++)
                header += reserved;
            //16-19 длина исходных данных
            header += source_length;
            //20-23 длина закодированных данных
            header += (char)(result_length / 256 / 256 / 256);
            //24-31 зарезервировано
            for (int i = 0; i < 8; i++)
                header += reserved;
            return header;
        }

        static string ReadFile(string path)
        {
            string date = "";
            if (File.Exists(path))

            {
                byte[] bytes = File.ReadAllBytes(path);
                foreach (byte b in bytes)
                {
                    date += (char)b;
                }
            }
            return date;
        }
        static void WriteFile(string path, string text)
        {
            byte[] bytes = new byte[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                bytes[i] = (byte)text[i];
            }
            File.WriteAllBytes(path, bytes);
        }
        public static byte[] StringToBytesArray(string str)
        {
            var bitsToPad = 8 - str.Length % 8;

            if (bitsToPad != 8)
            {
                var neededLength = bitsToPad + str.Length;
                str = str.PadLeft(neededLength, '0');
            }

            int size = str.Length / 8;
            byte[] arr = new byte[size];

            for (int a = 0; a < size; a++)
            {
                arr[a] = Convert.ToByte(str.Substring(a * 8, 8), 2);
            }

            return arr;
        }

        public static string bytesArrayToString(byte[] byteArray)
        {
            string yourByteString = "";
            for (int i = 0; i < byteArray.Length; i++)
                yourByteString += Convert.ToString(byteArray[i], 2).PadLeft(8, '0');

            return yourByteString;
        }

        //отсортировать список приоритетов по частоте для сборки дерева
        public static List<Tree> ordenar(List<Tree> a)
        {
            for (int i = 0; i < a.Count - 1; i++)
            {
                for (int j = i; j < a.Count; j++)
                {
                    if (Convert.ToInt32(a[i].frec) > Convert.ToInt32(a[j].frec))
                    {
                        Tree temp = a[i];
                        a[i] = a[j];
                        a[j] = temp;
                    }
                }
            }
            return a;
        }

        //найти, в какой индекс списка приоритетов нужно вставить значение с его периодичностью
        public static int buscarPosicion(List<Tree> a, Tree b)
        {
            int i = 0;
            while (i < a.Count)
            {
                if (Convert.ToInt32(b.frec) >= Convert.ToInt32(a[i].frec))
                {
                    i++;
                }
                else
                {
                    return i;
                }
            }
            return a.Count;
        }

        //читает дерево с частотой и возвращает каждый символ с его кодом Хаффмана
        public static List<Tree> recorridoArbol(Tree A)
        {
            List<Tree> pila = new List<Tree>();
            List<Tree> codificacion = new List<Tree>();
            A.frec = "";
            Tree temp;
            while (A != null || pila.Count > 0)
            {
                while (A != null)
                {
                    pila.Add(A);
                    temp = A;
                    A = A.hijoIzq;
                    if (A != null)
                        A.frec = temp.frec + "0";
                }
                if (pila.Count > 0)
                {

                    A = pila[pila.Count - 1];
                    pila.RemoveAt(pila.Count - 1);
                    if (A != null && A.letra != null)
                    {
                        Tree a = new Tree();
                        a.letra = A.letra;
                        a.frec = A.frec;
                        codificacion.Add(a);
                    }
                    temp = A;
                    A = A.hijoDer;
                    if (A != null)
                        A.frec = temp.frec + "1";

                }
            }
            return codificacion;
        }

        //сборка дерева, получение букв с их частотой и возвращение кода Хаффмана каждой буквы
        public static List<Tree> codificacionHuffman(List<Tree> codigoFrec)
        {
            //сначала вызваем sort, чтобы отсортировать список
            List<Tree> codigo = ordenar(codigoFrec);

            //Строится дерево
            while (codigo.Count > 1)
            {
                Tree a = new Tree();
                a.colocaIzq(codigo[0], a);
                a.colocaDer(codigo[1], a);
                codigo.RemoveAt(0);
                codigo.RemoveAt(0);
                a.frec = Convert.ToString(Convert.ToInt32(a.hijoDer.frec) + Convert.ToInt32(a.hijoIzq.frec));
                codigo.Insert(buscarPosicion(codigo, a), a);

            }

            //вызываем метод, который проходит по дереву и активирует код Хаффмана каждого символа
            codigo = recorridoArbol(codigo[0]);
            return codigo;
        }

        //восстановление дерева из кода каждой буквы, далее из этого дерева продолжается получение сжатого текста
        //окончательно вернуть распакованный текст
        public static string decodificacionHuffman(List<Tree> codificacion, string texto)
        {
            //восстанавливаем дерево
            Tree decodif = new Tree();

            for (int i = 0; i < codificacion.Count; i++)
            {

                string letra = codificacion[i].letra;

                string codigoBin = codificacion[i].frec;

                Tree temp = decodif;

                for (int j = 0; j < codigoBin.Length; j++)
                {
                    if (codigoBin[j] == '0')
                    {
                        if (temp.hijoIzq == null)
                            temp.hijoIzq = new Tree();
                        temp = temp.hijoIzq;
                    }
                    else
                    {
                        if (temp.hijoDer == null)
                            temp.hijoDer = new Tree();
                        temp = temp.hijoDer;
                    }

                    if (j == codigoBin.Length - 1)
                        temp.letra = letra;

                }
                temp = decodif;
            }

            //здесь расшифровываем текст
            string textDec = "";
            Tree reco = decodif;
            int k = 0;
            while (k < texto.Length)
            {
                if (texto[k] == '0')
                    reco = reco.hijoIzq;
                else
                    reco = reco.hijoDer;
                if (reco.letra != null)
                {
                    textDec += reco.letra;
                    reco = decodif;
                }
                k++;
            }

            return textDec;
        }

        // этот метод получает текст для сжатия и код Хаффмана каждого символа, возвращает текст только с 1 и 0
        public static string comprimir(string texto, List<Tree> claves)
        {
            string comprimido = "";
            for (int i = 0; i < texto.Length; i++)
            {
                for (int j = 0; j < claves.Count; j++)
                {
                    if (texto[i] == claves[j].letra[0])
                    {
                        comprimido += claves[j].frec;
                        break;
                    }
                }
            }
            return comprimido;
        }

        // получает текст, взятый из txt, и возвращает каждую букву с ее частотой
        public static List<Tree> contarFrec(string texto)
        {

            List<Tree> frecuencias = new List<Tree>();

            int cont = 0;
            char letra;
            while (texto.Length > 0)
            {
                letra = texto[0];

                for (int j = 0; j <= texto.Length - 1; j++)
                {
                    if (letra == texto[j])
                    {
                        cont++;
                        texto = texto.Remove(j, 1);
                        j--;
                    }
                }

                Tree temp = new Tree();
                temp.letra = letra.ToString();
                temp.frec = cont.ToString();
                frecuencias.Add(temp);

                cont = 0;
            }
            return frecuencias;
        }

        public static int Main(string[] args)
        {
            int mode = 0;
            string inputPath = "C:\\Users\\Alla\\Desktop\\test.txt";
            string outputPath = "C:\\Users\\Alla\\Desktop";

            //decompress

            //int mode = 1;
            //string inputPath = "C:\\Users\\Alla\\Desktop";
            //string outputPath = "C:\\Users\\Alla\\Desktop";



            List<Tree> codific = new List<Tree>();

            if (mode == 0)
            {

                string textOriginal = "";
                try
                {
                    string texto;
                    StreamReader sr = new StreamReader(inputPath);
                    texto = sr.ReadLine();
                    while (texto != null)
                    {
                        textOriginal += texto;
                        texto = sr.ReadLine();
                    }
                    sr.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("excepcion : " + ex.Message);
                }
                codific = contarFrec(textOriginal);

                string source = ReadFile(inputPath);
                int result_length = textOriginal.Length;
                int source_length = source.Length;
                Console.WriteLine("source_length: " + source_length);
                Console.WriteLine("result_length: " + result_length);

                List<Tree> a = codificacionHuffman(codific);

                //создаем текст с ключами
                try
                {
                    File.Delete(outputPath + "/keysFile.txt");
                    string fileName = outputPath + "/keysFile.txt";
                    StreamWriter writer = File.AppendText(fileName);

                    for (int i = 0; i < a.Count; i++)
                    {
                        writer.WriteLine(a[i].letra);
                        writer.WriteLine(a[i].frec);
                    }
                    writer.Close();
                }
                catch
                {
                    Console.WriteLine("Decoding Error");
                }

                try
                {
                    File.Delete(outputPath + "/compressedFile.txt");
                    string fileName = outputPath + "/compressedFile.txt";
                    StreamWriter writer = File.AppendText(fileName);

                    string textoCom = comprimir(textOriginal, a);

                    writer.WriteLine(textoCom);
                    writer.Close();

                    byte[] arr = StringToBytesArray(textoCom);

                    // узнаем, какие байты писать
                    // биты здесь - обычные заполнители, отбрасываем их при декодировании
                    int correctionFactor = (arr.Length * 8) - textoCom.Length;

                    //вставляем поправочный коэффициент в начале
                    byte[] newValues = new byte[arr.Length + 1];
                    newValues[0] = Convert.ToByte(correctionFactor);
                    Array.Copy(arr, 0, newValues, 1, arr.Length);
                    byte[] hdrNew = MakeHeaderNew(3, source_length, result_length);
                    //string header = MakeHeader(3, source_length, result_length);
                    //source = header + source;

                    Stream stream = new FileStream(outputPath + "/compressedFile.otik", FileMode.Create);
                    BinaryWriter bw = new BinaryWriter(stream);
                    bw.Write(hdrNew);
                    foreach (var b in newValues)
                    {
                        bw.Write(b);
                    }

                    bw.Flush();
                    bw.Close();
                }
                catch
                {
                    Console.WriteLine("Decoding Error");
                }

                Console.ReadKey();
            }
            else
            {
                //decod
                try
                {
                    string texto;
                    StreamReader sr = new StreamReader(inputPath + "/keysFile.txt");
                    texto = sr.ReadLine();
                    while (texto != null)
                    {
                        Tree temp = new Tree();
                        temp.letra = texto;
                        texto = sr.ReadLine();
                        temp.frec = texto;
                        codific.Add(temp);
                        Console.WriteLine(temp.letra + " " + temp.frec);
                        texto = sr.ReadLine();
                    }
                    sr.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("excepcion : " + ex.Message);
                }


                string textoBin = "";
                try
                {
                    string texto;
                    StreamReader sr = new StreamReader(inputPath + "/compressedFile.txt");
                    texto = sr.ReadLine();
                    while (texto != null)
                    {
                        texto = sr.ReadLine();
                    }
                    sr.Close();

                    byte[] file_data = null;
                    using (FileStream fs = new FileStream(inputPath + "/compressedFile.otik", FileMode.Open))
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        file_data = br.ReadBytes(Convert.ToInt32(fs.Length));
                    }
                    // удаляем из массива header перед продолжением декодирования
                    byte[] binaryArray = new byte[file_data.Length - 24];
                    for (int i = 24; i < file_data.Length; i++)
                        binaryArray[i - 24] = file_data[i];

                    int correctionFactor = binaryArray[0];
                    //int new_newlength = 0;
                    byte[] newValues = new byte[binaryArray.Length - 1];
                    // byte[] newnewValues = new byte[new_newlength];

                    Array.Copy(binaryArray, 1, newValues, 0, newValues.Length);
                    textoBin = bytesArrayToString(newValues);
                    Console.WriteLine(textoBin);
                    textoBin = textoBin.Substring(correctionFactor, textoBin.Length - correctionFactor);
                    Console.WriteLine(textoBin);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("excepcion : " + ex.Message);
                }
                try
                {
                    File.Delete(outputPath + "/decompressedFile.txt");
                    string fileName = outputPath + "/decompressedFile.txt";
                    StreamWriter writer = File.AppendText(fileName);

                    writer.WriteLine(decodificacionHuffman(codific, textoBin));

                    writer.WriteLine("");

                    writer.Close();
                }
                catch
                {
                    Console.WriteLine("Decoding Error");
                }

                Console.ReadKey();

            }
            return 0;
        }
    }
}

