# PaddleOCR
# 记录

## 框架

## 1、目标（物体与激光点）识别，获取位置
## 2、PTZ摄像机与双轴高精度激光测距模块控制
## 3、目标点与激光点匹配；
### 3.1、初步计算与匹配
### 3.2、旋转到物体位置；
### 3.3、计算球积到图像中心的偏差，并确定旋转角度，球积与全站仪同步旋转
#### 3.3.1 采集第一张图像，计算物体特征点，移动到图像中心，采集第二张图像，确定物体特征点已经移动到图像中心，并且计算激光点位置，计算全站仪旋转角度。
### 3.4、追踪完成，确定球积放大倍率，放大图像；
### 3.5、进行位置重新获取，微调两个位置

### 3.6、将运动信息保存引入放大图像，进行位置映射
### 3.7、编辑
### 2025年2月11日 确定初步匹配流程；采集移动放大后的位置偏差图，带入放大率进行计算
### 2025年2月12日 根据激光点与物体间的偏差，设计调整算法。反馈PID调整算法
####    将推理出来的矩形框充分利用，包括长宽，分别移动到矩形框的四个坐标。移动后的分辨率均为一致，其放大率发生变化。
####    调平台：静止波动0.01度。
####    放大到最15倍时，检测槽道与激光点，激光放大倍率。能够检测到槽道。
####    1、获取矩形的4个顶点，计算旋转角度，分别同步旋转至四个位置，对四个槽道的尖角位置精确定位，获取空间点，计算长与两个间距；
####    2检测到隧道中线后，相邻100mm生成3个直线，并依次按着旋转三个直线的上顶点，从上顶点位置每隔2mm进行扫描，获得点位数列；
####    3、将点位进行拟合计算槽宽，槽深。
####    4、隧道中心检测算法、槽道四顶点尖角检测算法
### 2025年2月13日 移动到扫描起始位置，开始横向扫描获取点位，
### 2025年2月14日 将扫描的点位进行空间数据整形：模拟20个点位进行输出
mermaid
graph TD
    A[原始数据] --> B[数据滤波]
    B --> C[坐标转换]
    C --> D[边缘检测]
    D --> E[宽度计算]
    D --> F[深度计算]
    E --> G[输出结果]
    F --> G
[========]

### 绘制流程图 Flowchart

```flow
st=>start: 用户登陆
op=>operation: 登陆操作
cond=>condition: 登陆成功 Yes or No?
e=>end: 进入后台

st->op->cond
cond(yes)->e
cond(no)->op
```

[========]

$$E=mc^2$$
