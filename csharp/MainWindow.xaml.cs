using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CORTool
{
    // 定义 C# 中的对应数据结构
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct OCRPredictResult
    {
        public IntPtr box;  // 指向二维数组的指针
        public IntPtr innersize; // box 数组的大小
        public int outersize; // box 数组的大小
        public IntPtr text; // 指向字符串的指针
        public IntPtr imagename; // 指向字符串的指针
        public IntPtr detimagename; // 指向字符串的指针
        public float score;
        public float cls_score;
        public int cls_label;
    }
    public struct OCRPredictResultUI
    {
        public List<List<int>> box;  
        public string text;
        public string imagename;
        public string detimagename;
        public float score;
        public float cls_score;
        public int cls_label;
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        [System.Runtime.InteropServices.DllImport("ppocr.dll", SetLastError = true,CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void ocr(
            string clsmodeldir,
            string detmodeldir, 
            string recmodeldir,
            bool cls, 
            bool rec, 
            bool det, 
            string dickfile,
            string imagedir,
            IntPtr results,
            IntPtr innersize,
            out int outersize);
        [System.Runtime.InteropServices.DllImport("ppocr.dll", SetLastError = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void ocrfreememory(
            IntPtr results,
            IntPtr innersize,
            out int outersize);

        void OCR(string imgdir)
        {
            IntPtr resultsPtr;
            int outerSize;
            IntPtr innerSizesPtr;

            // 分配内存
            resultsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            innerSizesPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));

            ocr(
                @"D:\AIPlus\OCR\ch_ppocr_mobile_v2.0_cls_infer\",
                @"D:\AIPlus\OCR\ch_PP-OCRv4_det_infer\",
                @"D:\AIPlus\OCR\ch_PP-OCRv4_rec_infer\",
                false, true, true,
                @"D:\AIPlus\OCR\ppocr_keys_v1.txt",
                @"D:\AIPlus\OCR\",
                resultsPtr,
                innerSizesPtr,
                out outerSize);


            // 解析结果
            OCRPredictResultUI[][] results = new OCRPredictResultUI[outerSize][];
            int[] innerSizes = new int[outerSize];
            IntPtr innerArrayPtr = Marshal.ReadIntPtr(innerSizesPtr);
            for (int i = 0; i < outerSize; i++)
            {
                innerSizes[i] = Marshal.ReadInt32(innerArrayPtr, i * sizeof(int));
            }

            IntPtr innerResultsPtr = Marshal.ReadIntPtr(resultsPtr);
            for (int i = 0; i < outerSize; i++)
            {

                results[i] = new OCRPredictResultUI[innerSizes[i]];
                IntPtr eachresultptr = Marshal.ReadIntPtr(innerResultsPtr, i * IntPtr.Size);
                for (int j = 0; j < innerSizes[i]; j++)
                {
                    IntPtr resultPtr = eachresultptr + j * Marshal.SizeOf<OCRPredictResult>();
                    OCRPredictResult result = Marshal.PtrToStructure<OCRPredictResult>(resultPtr);

                    results[i][j].cls_label = result.cls_label;
                    results[i][j].cls_score = result.cls_score;
                    results[i][j].score = result.score;
                    results[i][j].text = Marshal.PtrToStringAnsi(result.text);
                    results[i][j].imagename = Marshal.PtrToStringAnsi(result.imagename);
                    results[i][j].detimagename = Marshal.PtrToStringAnsi(result.detimagename);

                    Console.WriteLine(results[i][j].text);
                    results[i][j].box = new List<List<int>>();

                    int[] boxinnerSizes = new int[result.outersize];
                    for (int k = 0; k < result.outersize; k++)
                    {
                        boxinnerSizes[k] = Marshal.ReadInt32(result.innersize, k * sizeof(int));
                    }

                    for (int m = 0; m < result.outersize; m++)
                    {
                        IntPtr eachboxlistptr = Marshal.ReadIntPtr(result.box, m * IntPtr.Size);
                        List<int> pts = new List<int>();
                        for (int u = 0; u < boxinnerSizes[m]; u++)
                        {
                            pts.Add(Marshal.ReadInt32(eachboxlistptr, u * sizeof(int)));
                        }
                        results[i][j].box.Add(pts);
                    }
                }
            }


            //释放C++中申请的内存
            ocrfreememory(
                resultsPtr,
                innerSizesPtr,
                out outerSize);
            // 释放内存
            Marshal.FreeHGlobal(resultsPtr);
            Marshal.FreeHGlobal(innerSizesPtr);

            this.mReady = true;
            if (results.Length > 0)
            {
                this.m_Results = results;
                this.mCurIndex = 0;
                this.ShowOCR(this.m_Results[0]);              
            }
           
        }

        void ShowOCR(OCRPredictResultUI[] ocrs)
        {
            if (ocrs.Count() == 0)
                return;

            if (this.mReady == false)
                return;

            string imagename = ocrs.First().imagename;
            if (!imagename.EndsWith(".jpg") && !imagename.EndsWith(".png"))
                return;

            string detimagename = ocrs.First().detimagename;
            if (!detimagename.EndsWith(".jpg") && !detimagename.EndsWith(".png"))
                return;

            BitmapImage bitmap = new BitmapImage(new Uri(detimagename));
            this.mImage.Source = bitmap;//显示图像

            StringBuilder sb = new StringBuilder();
            foreach (var v in ocrs)
            {
                sb.AppendLine(v.text);
            }

            this.mOcrtxt.Text = sb.ToString();
        }
      

        OCRPredictResultUI[][] m_Results;
        int mCurIndex = 0;
        bool mReady = false;

        public MainWindow()
        {
            InitializeComponent();

            this.mImage.Width = this.Width / 2;
            this.mImage.Height = this.Height * 0.9f;
        }
       
        private void ImgSelButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "选择文件夹";

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedFilePath = folderBrowserDialog.SelectedPath;
                Console.WriteLine("选择的文件路径：" + selectedFilePath);
                this.mReady = false;
                this.OCR(selectedFilePath);
            }

        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int sliceNum = this.m_Results.Length;
            int curindex = this.mCurIndex;
            if (e.Delta > 0)
            {
                if (--curindex < 0)
                    return;

            }
            else if (e.Delta < 0)
            {
                if (++curindex >= sliceNum)
                    return;

            }
            this.mCurIndex = curindex;//保存当前层index

            this.ShowOCR(this.m_Results[this.mCurIndex]);

        }
    }
}
