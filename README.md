# DependenciesSelector
## 简介
这是一个在Unity中用于寻找指定prefab所依赖的所有资源的插件，内置了TortoiseSVN提供的指令，可以很方便地提交prefab所依赖的所有资源，也添加了meta文件的检查功能，能避免遗漏提交meta文件。

## 环境要求
* Windows
* TortoiseSVN客户端（版本最好在1.7以上）
* Unity 5.x +

## 使用方法
* 将`Editor`目录导入Unity项目中
* 在`Project`视图中，右键选中需要进行操作的prefab，选择菜单中的`Find Prefab Dependencies`打开操作窗口
* 点击`Check Dependencies`按钮搜索prefab依赖的所有资源
* 选择需要操作的文件
    * 点击`Copy Resources`并选择目标文件夹复制选中的文件
    * 点击`Commit`按钮打开`SVN Tool`操作窗口，`由于在提交文件之前，需要将该文件所在的文件夹添加到版本控制，所以需要手动选择新建的文件夹进行提交`

## 展示截图
![操作窗口](https://github.com/AsanCai/DependenciesSelector/raw/master/Images/image1.png)
![选择目标文件夹](https://github.com/AsanCai/DependenciesSelector/raw/master/Images/image2.png)
![SVN Tool](https://github.com/AsanCai/DependenciesSelector/raw/master/Images/image3.png)
![选择需要提交的文件](https://github.com/AsanCai/DependenciesSelector/raw/master/Images/image4.png)

## 其他
由于TortoiseSVN提供的文档很少，且其[GUI程序的指令](https://tortoisesvn.net/docs/nightly/TortoiseSVN_zh_CN/tsvn-automation.html)与Subversion命令行客户端的指令相差较大，所以目前只提供了简单的`Add`和`Commit`功能。
此外，因为在将文件夹添加到版本控制时，默认会将其包含的子目录和文件全部添加到版本控制里，因此目前仍会出现提交时，提交面板多出了一些自己未选中的`non-version文件`，后面的版本将会对这个问题进行优化。
