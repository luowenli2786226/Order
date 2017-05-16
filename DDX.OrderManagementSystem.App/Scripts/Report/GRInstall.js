//在网页中引用此文件之前，应该先引用CreateControl.js
//document.write("<script type='text/javascript' src='CreateControl.js'></script>");

//插入一个报表对象，用来判断插件是否已经安装，或是否需要安装更新版本
//此函数应该在网页的<head>中调用，具体请看例子 ReportHome.htm 中的用法
function Install_InsertReport()
{
    var typeid;
    if( _gr_isIE )
        typeid = 'classid="clsid:25240C9A-6AA5-416c-8CDA-801BBAF03928" ';
    else
        typeid = 'type="application/x-grplugin-report" ';
    typeid += gr_CodeBase;
	document.write('<object id="_ReportOK" ' + typeid);
	document.write(' width="0" height="0" VIEWASTEXT>');
	document.write('</object>');
}

//用来判断插件是否已经安装，或是否需要安装更新版本。如果需要安装，则在网页中插入安装相关的文字内容
//如果插件已经安装且也不要更新，则返回 true，反之为 false。
//此函数应该在网页的<body>开始位置处调用，具体请看例子 ReportHome.htm 中的用法
function Install_Detect()
{
    var _ReportOK = document.getElementById("_ReportOK");
    if ((_ReportOK == null) || (_ReportOK.Register == undefined))
    {
       
            
     
        return false;
    }
    else if ((_ReportOK.Utility.ShouldUpdatePlugin == undefined) || _ReportOK.Utility.ShouldUpdatePlugin(gr_Version) == true)  //检查是否应该下载新版本程序
    {
       
        return false;
    }
    
    return true;
}
