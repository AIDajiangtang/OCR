# OCR
Deeplearning based OCR


本项目的核心是C++代码，基于paddle ocr，负责加载模型，推理，结果后处理。  

C++代码支持跨平台，但目前仅在Windows平台测试过。  
如何编译请参照：  
https://github.com/PaddlePaddle/PaddleOCR/blob/release/2.7/deploy/cpp_infer/docs/windows_vs2019_build.md

编译完成后生成ppocr.exe.  
在控制台输入下面命令行代码运行程序。  
 ppocr.exe --det_model_dir=D:\AIPlus\OCR\ch_PP-OCRv4_det_infer\ --rec_model_dir=D:\AIPlus\OCR\ch_PP-OCRv4_rec_infer\  --image_dir=D:\AIPlus\OCR\12.jpg --rec_char_dict_path=D:\AIPlus\OCR\ppocr_keys_v1.txt --use_angle_cls=false --det=true --rec=true --cls=false

<img width="500" src="https://user-images.githubusercontent.com/18625471/266959787-53e44c71-089a-4170-9ec0-9ab393de4d4a.jpg">  

<img width="500" src="https://user-images.githubusercontent.com/18625471/266959803-95041be4-14c3-460d-9e49-d9fb8a80be58.jpg">  

<img width="500" src="https://user-images.githubusercontent.com/18625471/266959820-3fd16728-dff2-446e-9800-46c3a332fbfb.jpg">  


 除此之外，可以CmakeLists.txt中add_executable(${DEMO_NAME} ${SRCS})改成add_library(${DEMO_NAME} SHARED ${SRCS})，将上述C++代码编译成动态链接库ppocr.dll，然后在C#中调用。

 <img width="500" src="https://user-images.githubusercontent.com/18625471/266960542-0959d676-0412-4b2f-8c4b-a07907987ba4.png">  


微信公众号回复【ocr】获取安装包以及模型  
微信公众号：**人工智能大讲堂**  
<img width="180" src="https://user-images.githubusercontent.com/18625471/228743333-77abe467-2385-476d-86a2-e232c6482291.jpg">  

安装包中CORTool.exe为C#可执行程序  
ppocr.exe为C++可执行程序  
ppocr.dll为c++动态链接库  
ch_ppocr_mobile_v2.0_cls_infer为分类模型文件夹  
ch_PP-OCRv4_det_infer为检测模型文件夹  
ch_PP-OCRv4_rec_infer为识别模型文件夹  
ppocr_keys_v1.txt为检测模型字典  
上述文件夹和文件分别对应下面函数的参数  

导出函数为：
extern "C" __declspec(dllexport)
void ocr(
    char* clsmodeldir, 
    char* detmodeldir, 
    char* recmodeldir, 
    bool cls, 
    bool rec, 
    bool det, 
    char* dickfile,
    char* imagedir, 
    OCRPredictResultUI ***results,
    int** innersize,
    int* outersize)
