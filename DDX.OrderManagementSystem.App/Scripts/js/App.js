﻿
/******
定义各种参数
*/
var checks = [["1", "是"], ["2", "否"]];
var checksall = [["0", "===请选择==="], ["1", "是"], ["2", "否"]];
var checkSex = [["男", "男"], ["女", "女"]];;


// easyUI  时间改写
Date.prototype.format = function (format) {
    var o = {
        "M+": this.getMonth() + 1, //month
        "d+": this.getDate(), //day
        "h+": this.getHours(), //hour
        "m+": this.getMinutes(), //minute
        "s+": this.getSeconds(), //second
        "q+": Math.floor((this.getMonth() + 3) / 3), //quarter
        "S": this.getMilliseconds() //millisecond
    }
    if (/(y+)/.test(format)) format = format.replace(RegExp.$1,
    (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o) if (new RegExp("(" + k + ")").test(format))
        format = format.replace(RegExp.$1,
        RegExp.$1.length == 1 ? o[k] :
        ("00" + o[k]).substr(("" + o[k]).length));
    return format;
}
function getDateTime(value) {
    if (!value) return "";
    var date = new Date(parseInt(value.replace("/Date(", "").replace(")/", ""), 10));
    return date.format("yyyy-MM-dd hh:mm:ss");
}

function getDate(value) {
    if (!value) return "";
    var date = new Date(parseInt(value.replace("/Date(", "").replace(")/", ""), 10));
    return date.format("yyyy-MM-dd");
}
function getStartDateTime(value, t) {
    var today = new Date();
    var enddate;
    if (t == "d")
        enddate = getDate(today.setDate(today.getDate() - value).toString());
    else {
        enddate = getDate(today.setHours(today.getHours() - value).toString());
    }
    return enddate;
}

function getStartDate(value) {
    if (!value) return "";
    var today = new Date();
    var enddate = getDate(today.setDate(today.getDate() - value).toString());
    return enddate;
}

function getSmallImg(v) {
    return '<img  src=' + v + '  height="64px" width="64px" />';
}

function getTrue(value) {
    if (value == 1)
        return "是";
    else
        return "否";
}

function ajaxFrom(form, url) {
    $.ajax({
        url: url,
        type: "Post",
        data: $("#" + form).serialize(),
        dataType: "json",
        success: function (data) {
            if (data.IsSuccess) {

            }
        }
    });
}

//获取选中行
function getselectedRow(gird) {
    var row = gird.datagrid('getSelected');
    if (row != undefined) {
        if (row.hasOwnProperty('Id')) {
            var id = row['Id'];
            return id;
        }
    }
    $.messager.alert('操作提示', '请选择数据!', 'warning');
    return (undefined);
}

//获取选中行(多行)
function getselectedRows(gird, c) {
    if (!c) c = "Id";
    var rows = gird.datagrid('getSelections');
    var ids = [];
    for (var i = 0; i < rows.length; i++) {
        ids.push(rows[i][c]);
    }
    if (ids.length == 0)
        $.messager.alert('操作提示', '请选择数据!', 'warning');
    else
        return ids;
    return (undefined);
}

//删除的按钮
function del(grid, url, t) {
    var rows = grid.datagrid('getSelections');
    if (rows.length == 0) {
        $.messager.alert('操作提示', '请选择数据!', 'warning');
        return false;
    }
    var arr = [];
    for (var i = 0; i < rows.length; i++) {
        arr.push(rows[i].Id);
    }
    $.messager.confirm('操作提示', "确认删除这 " + arr.length + " 项吗？", function (r) {
        if (r) {
            $.post(url, { id: arr.join(",") }, function (res) {
                if (res.IsSuccess) {
                    //移除删除的数据
                    $.messager.alert('操作提示', '删除成功!', 'info');
                    if (t == 'tg') {
                        grid.treegrid("reload");
                        grid.treegrid("clearSelections");
                    } else {
                        grid.datagrid("reload");
                        grid.datagrid("clearSelections");
                    }
                }
                else {
                    if (res.Result == "") {
                        $.messager.alert('操作提示', '删除失败!请查看该数据与其他模块下的信息的关联，或联系管理员。', 'info');
                    }
                    else {
                        $.messager.alert('操作提示', res, 'info');
                    }
                }
            });
        }
    });
};

function comboboxInit(url, postdata, combo, vf, tf, w, h, ph, m) {
    $.ajax({
        url: url,
        type: "post",
        data: postdata,
        success: function (data) {
            $('#' + combo).combobox({
                data: data.rows,
                valueField: vf,
                textField: tf,
                multiple: m || false,
                panelHeight: ph || 'auto',
                width: w || 150,
                onLoadSuccess: function () { //加载完成后,设置选中第一项
                    if (undefined == $(this).combobox("getValue") || $(this).combobox("getValue").length == 0) {
                        var val = $(this).combobox("getData");
                        for (var item in val[0]) {
                            if (item == vf) {
                                $(this).combobox("select", val[0][item]);
                            }
                        }
                    }
                },
                onSelect: function (r) {
                    if (h) {
                        h(r);
                    }
                }
            });
        }
    });
}

//messagex

function showSuccessMessage() {
    MessagetopCenter("设置成功!");
}
function showErrorMessage() {
    MessagetopCenter("设置失败!");
}

function MessagetopCenter(m, t) {
    t = t || "提示";
    $.messager.show({
        title: t,
        msg: m,
        showType: 'slide',
        style: {
            right: '',
            top: (document.body.scrollTop + document.documentElement.scrollTop),
            bottom: ''
        },
        timeout: 2000
    });
}

/**
 * @author 孙宇
 * 
 * @requires jQuery,EasyUI
 * 
 * 创建一个模式化的dialog
 * 
 * @returns $.modalDialog.handler 这个handler代表弹出的dialog句柄
 * 
 * @returns $.modalDialog.xxx 这个xxx是可以自己定义名称，主要用在弹窗关闭时，刷新某些对象的操作，可以将xxx这个对象预定义好
 */
$.modalDialog = function (options) {
    if ($.modalDialog.handler == undefined) {// 避免重复弹出
        var opts = $.extend({
            title: '',
            width: 840,
            height: 680,
            modal: true,
            onClose: function () {
                $.modalDialog.handler = undefined;
                $(this).dialog('destroy');
            },
            onOpen: function () {
                //parent.$.messager.progress({
                //    title: '提示',
                //    text: '数据处理中，请稍后....'
                //});
            }
        }, options);
        opts.modal = true;// 强制此dialog为模式化，无视传递过来的modal参数
        return $.modalDialog.handler = $('<div/>').dialog(opts);
    }
};

$.extend($.fn.datagrid.defaults.editors, {
    textarea: {
        //textarea就是你要自定义editor的名称
        init: function (container, options) {
            var me = this;
            var cur = $('<input id="inputID" type="text" class="datagrid-editable-input" /> ');
            var editor = cur.appendTo(container);
            $(cur).focus(function () {

                $("#textareaID").val($("#inputID").val().replace('<br />', '\n'));
                var textareaWin = $('#winnn').dialog({
                    title: '编辑单元格',
                    width: 350,
                    height: 220,
                    closable: false,
                    resizable: false,
                    closed: false,
                    collapsible: false,
                    maximizable: false,
                    minimizable: false,
                    modal: true,
                    buttons: [{
                        text: '保存',
                        iconCls: 'icon-add',
                        handler: function () {
                            var textVal = $("#textareaID").val();
                            textVal = textVal.replace('\n', '<br />');
                            $("#inputID").val(textVal);

                            textareaWin.window('close');
                            textareaWin = undefined;
                        }
                    }, {
                        iconCls: 'icon-reject',
                        text: '取消',
                        handler: function () {
                            $.messager.confirm('【提示信息】', '是否确认退出编辑？', function (r) {
                                if (r) {
                                    textareaWin.window('close');
                                    textareaWin = undefined;
                                }
                            });

                        }
                    }]
                });
                $("#textareaID").focus();

            });
            console.log("init method invoke!");
            editor.enableEdit = false;
            return editor;
        },
        getValue: function (target) {
            return $(target).val();
        },
        setValue: function (target, value) {
            $(target).val(value);
        },
        resize: function (target, width) {
            console.log("resize method invoke!");
        },
        destroy: function (target) {
            console.log("destroy method invoke!");
            textareaWin = undefined;
        }
    },
    combotree: {
        init: function (container, options) {
            var editor = jQuery('<input type="text">').appendTo(container);
            editor.combotree(options);
            return editor;
        },
        destroy: function (target) {
            jQuery(target).combotree('destroy');
        },
        getValue: function (target) {
            var temp = jQuery(target).combotree('getValues');
            //alert(temp);  
            return temp.join(',');
        },
        setValue: function (target, value) {
            var temp = value.split(',');
            //alert(temp);  
            jQuery(target).combotree('setValues', temp);
        },
        resize: function (target, width) {
            jQuery(target).combotree('resize', width);
        }
    }

});
//编辑表格中的datagrid
$.extend($.fn.datagrid.defaults.editors, {
    combogrid: {
        init: function (container, options) {
            var input = $('<input type="text" class="datagrid-editable-input">').appendTo(container);
            input.combogrid(options);
            return input;
        },
        destroy: function (target) {
            $(target).combogrid('destroy');
        },
        getValue: function (target) {
            return $(target).combogrid('getValue');
        },
        setValue: function (target, value) {
            $(target).combogrid('setValue', value);
        },
        resize: function (target, width) {
            $(target).combogrid('resize', width);
        }
    }
});
//跳出弹窗
function showdlg(url, dlg, handle) {
    var downloadHelper = $('<iframe style="display:none;" id="dlg"></iframe>').appendTo('body')[0];
    $('#' + dlg).load(url, function () {
        $(this).dialog({
            title: '新建',
            modal: true,
            rownumbers: true,
            loadingMessage: '正在加载...',
            buttons: [{
                text: '提交',
                iconCls: 'icon-ok',
                handler: handle
            }, {
                text: '取消',
                handler: function () {
                    $('#' + dlg).dialog('close');
                }
            }]
        });
    }).dialog('open');
}

createFrame = function (url) {
    var s = '<iframe scrolling="auto" frameborder="0"  src="' + url + '" style="width:100%;height:99.5%"></iframe>';
    return s;
};
openTab = function (subtitle, url, closable) {

    var self = top.jQuery("#index_tabs");
    if (!self.tabs('exists', subtitle)) {
        var cl = true;
        if (closable == 'false')
            cl = false;
        self.tabs('add', {
            title: subtitle,
            content: createFrame(url),
            closable: cl
        });
    } else {
        self.tabs('select', subtitle);
    }
    return self;
};


function editGridViewModel(grid, saveurl, delurl, f) {
    var self = self || {};
    self.begin = function (index, row) {
        if (index == undefined || typeof index === 'object') {
            row = grid.datagrid('getSelected');
            index = grid.datagrid('getRowIndex', row);
        }
        self.editIndex = index;
        grid.datagrid('selectRow', self.editIndex).datagrid('beginEdit', self.editIndex);
    };
    self.ended = function () {
        if (self.editIndex == undefined) return true;
        if (grid.datagrid('validateRow', self.editIndex)) {
            grid.datagrid('endEdit', self.editIndex);
            if (!f) {
                $.post(saveurl, grid.datagrid("getRows")[self.editIndex], function (rsp) {
                    if (rsp.IsSuccess) {
                        $.messager.show({
                            title: '提示',
                            msg: "操作成功",
                            timeout: 2000,
                            showType: 'slide'
                        });
                    }
                }, "JSON").error(function () {
                    $.messager.show({
                        title: '提示',
                        msg: "操作失败",
                        timeout: 2000,
                        showType: 'slide'
                    });
                });
            }
            self.editIndex = undefined;
            return true;
        }
        grid.datagrid('selectRow', self.editIndex);
        return false;
    };

    self.addnew = function (rowData) {
        if (self.ended()) {
            if (Object.prototype.toString.call(rowData) != '[object Object]') rowData = {};
            rowData = $.extend({ _isnew: true }, rowData);
            grid.datagrid('appendRow', rowData);
            self.editIndex = grid.datagrid('getRows').length - 1;
            grid.datagrid('selectRow', self.editIndex);
            self.begin(self.editIndex, rowData);
        }
    };

    self.deleterow = function () {
        var selectRow = grid.datagrid('getSelected');
        if (selectRow) {
            var selectIndex = grid.datagrid('getRowIndex', selectRow);
            if (selectIndex == self.editIndex) {
                grid.datagrid('cancelEdit', self.editIndex);
                self.editIndex = undefined;

            }
            if (!f) {
                $.post(delurl, "id=" + selectRow.Id, function (rsp) {
                    if (rsp.IsSuccess) {
                        $.messager.show({
                            title: '提示',
                            msg: "删除成功",
                            timeout: 2000,
                            showType: 'slide'
                        });
                    }
                }, "JSON").error(function () {
                    $.messager.show({
                        title: '提示',
                        msg: "删除失败",
                        timeout: 2000,
                        showType: 'slide'
                    });
                });
            }
            grid.datagrid('deleteRow', selectIndex);
        }
    };
    self.reject = function () {
        grid.datagrid('rejectChanges');
    };
    self.accept = function () {
        if (self.ended()) {
            grid.datagrid('acceptChanges');
            var rows = grid.datagrid('getRows');
            for (var i in rows) delete rows[i]._isnew;
        }
    };
    self.getChanges = function (include, ignore) {
        if (!include) include = [], ignore = true;

        return JSON.stringify(grid.datagrid("getRows"));

    };
    self.isChangedAndValid = function () {
        return self.ended() && self.getChanges()._changed;
    };
    return self;
};

