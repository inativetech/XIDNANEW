var str; var res;
var UserID;
var RoleID;
var i = 0;
var UserSigRCategory;
$(function () {
    $('.treeview').removeClass("priorityHighFlash");
    SignalRInboxCount();
    GetNotificationCount();
    GetLeadData();
    // Declare a proxy to reference the hub.
    var notifications = $.connection.notifyHub;
    // Create a function that the hub can call to broadcast messages Count Start.
    notifications.client.EntireSolution = function (CountofList) {
        var i = 0;
        //if (CountofList.length > 0) {
        //    for (i = 0; i < CountofList.length; i++) {
        //        str = CountofList[i];
        if (CountofList != null) {
            str = CountofList;
            res = str.split("_");
            var ID = parseInt(res[1]);
            var NumPB = res[2];
            var IDhereORnot = $('#Menu-' + ID);
            if (IDhereORnot && IDhereORnot.length > 0) {
                if (NumPB && NumPB == "Number") {
                    var PreviousCount = parseInt(IDhereORnot[0].innerHTML);
                    var PresentCount = parseInt(res[0]);
                    IDhereORnot.html(PresentCount);
                    if (PreviousCount < PresentCount) {
                        $('#' + ID).addClass('priorityHighFlash');
                        setTimeout(function () {
                            //IDhereORnot.removeClass("priorityHighFlash");
                            $('.treeview').removeClass("priorityHighFlash");
                        }, 5000);
                    }
                }
                else if (NumPB && NumPB == "Progress Bar") {
                    var previousPB = $('.progress-bar').attr('style');
                    //var Round = Math.round(res[0] / 100);
                    var PresetPB = 'width:' + (res[0] / 100) + '%';
                    if (PresetPB != previousPB) {
                        $('#' + ID).addClass('priorityHighFlash');
                    }
                    setTimeout(function () {
                        $('.treeview').removeClass("priorityHighFlash");
                    }, 5000);
                }
            }
        }

    };
    notifications.client.BroadcastNotification = function (config) {
        debugger;
        var UserCategory = UserSigRCategory.split(',');
        var UserCategories =[];
        for(var i=0;i<UserCategory.length;i++){
            UserCategories.push(parseInt(UserCategory));
        }
        if (UserCategories.indexOf(config.iCategory) > -1) {
            //In the array!
        } else {
            //Not in the array
            return false;
        }
        var currentdate = new Date();
        var datetime = +currentdate.getFullYear() + "-"
                        + (currentdate.getMonth() + 1) + "-"
                        + currentdate.getDate() + "T"
                        + currentdate.getHours() + ":"
                        + currentdate.getMinutes() + ":"
                        + currentdate.getSeconds();
        if (config != null) {
            if (config.ShowCount == 30) {
                $('#' + config.ConstantID).html(config.Count);
            }
            else {
                $('#' + config.ConstantID).empty();
            }
            if (config.UserID == -1) {
                  if (config.Config.toLowerCase() == "flash") {
                        //debugger;
                        if (config.WhereFields != null && config.MasterID != "" && config.MasterID != null && config.AlertText != "") {
                            EasyAlert.Easy({
                                text: config.AlertText,
                                time: "5000",
                                bkgrndColour: "#ee6363",
                                textColour: "#FFFFFF"
                            });
                        }
                    //else if (config.WhereFields == "[ACPolicy_T].iStatus = '10'" && config.MasterID != null) {
                    //    EasyAlert.Easy({
                    //        text: "Insert new Policy " + config.MasterID,
                    //        time: "5000",
                    //        bkgrndColour: "#6363EE",
                    //        textColour: "#FFFFFF"
                    //    });
                    //}
                    //else if (config.WhereFields == "[ACPolicy_T].iStatus = '50'" && config.MasterID != null) {
                    //    EasyAlert.Easy({
                    //        text: "Insert Rebroke Policy " + config.MasterID,
                    //        time: "5000",
                    //        bkgrndColour: "#fcf0ff",
                    //        textColour: "#FFFFFF"
                    //    });
                    //}
                    //else if (config.WhereFields == "[ACPolicy_T].iStatus = '25'" && config.MasterID != null) {
                    //    EasyAlert.Easy({
                    //        text: "Insert Renewal Policy " + config.MasterID,
                    //        time: "5000",
                    //        bkgrndColour: "#FF8C00",
                    //        textColour: "#FFFFFF"
                    //    });
                    //}
                    //else if (config.WhereFields == "[ACPolicy_T].iStatus = '200'" && config.MasterID != null) {
                    //    EasyAlert.Easy({
                    //        text: "Insert Cancelled Policy " + config.MasterID,
                    //        time: "5000",
                    //        bkgrndColour: "#009999",
                    //        textColour: "#FFFFFF"
                    //    });
                    //}
                }
                  else if (config.Config.toLowerCase() == "flashnotification") {
                      //debugger;
                      if (config.WhereFields != null && config.MasterID != "" && config.MasterID != null && config.AlertText != "") {
                          EasyAlert.Easy({
                              text: config.AlertText,
                              time: "5000",
                              bkgrndColour: "#ee6363",
                              textColour: "#FFFFFF"
                          });
                      }
                  }
            }
            if (config.UserID == parseInt(UserID) || config.iRoleID==parseInt(RoleID)) {
                //debugger;
                if (datetime >= config.NotificationOFFFrom && datetime <= config.NotificationOFFTo) {

                }
                else {
                    if (config.Config.toLowerCase() == "popup" || config.Config.toLowerCase() == "dialog") {
                        PopupID(config.UserID, config.ResultOneClick, config.Config);
                    }
                        //else if (config.Config.toLowerCase() == "dialog".toLowerCase()) {
                        //    PopupID(config.UserID, config.ResultOneClick, config.Config);
                        //}
                    else if (config.Config.toLowerCase() == "alert") {
                        window.alert(config.AlertText);
                    }
                    else if (config.Config.toLowerCase() == "flash") {
                        //debugger;
                        if (config.WhereFields != null && config.MasterID != "" && config.MasterID != null && config.AlertText != "") {
                            EasyAlert.Easy({
                                text: config.AlertText,
                                time: "5000",
                                bkgrndColour: "#ee6363",
                                textColour: "#FFFFFF"
                            });
                        }
                        //    else if (config.WhereFields == "[ACPolicy_T].iStatus = '10'" && config.MasterID != null) {
                        //        EasyAlert.Easy({
                        //            text: "Insert new Policy " + config.MasterID,
                        //            time: "5000",
                        //            bkgrndColour: "#6363EE",
                        //            textColour: "#FFFFFF"
                        //        });
                        //    }
                        //    else if (config.WhereFields == "[ACPolicy_T].iStatus = '50'" && config.MasterID != null) {
                        //        EasyAlert.Easy({
                        //            text: "Insert Rebroke Policy " + config.MasterID,
                        //            time: "5000",
                        //            bkgrndColour: "#fcf0ff",
                        //            textColour: "#FFFFFF"
                        //        });
                        //    }
                        //    else if (config.WhereFields == "[ACPolicy_T].iStatus = '25'" && config.MasterID != null) {
                        //        EasyAlert.Easy({
                        //            text: "Insert Renewal Policy " + config.MasterID,
                        //            time: "5000",
                        //            bkgrndColour: "#FF8C00",
                        //            textColour: "#FFFFFF"
                        //        });
                        //    }
                        //    else if (config.WhereFields == "[ACPolicy_T].iStatus = '200'" && config.MasterID != null) {
                        //        EasyAlert.Easy({
                        //        text: "Insert Cancelled Policy " + config.MasterID,
                        //        time: "5000",
                        //        bkgrndColour: "#009999",
                        //        textColour: "#FFFFFF"
                        //    });
                        //}
                    }
                    else if (config.Config.toLowerCase() == "flashnotification") {
                        //debugger;
                        if (config.WhereFields != null && config.MasterID != "" && config.MasterID != null && config.AlertText != "") {
                            EasyAlert.Easy({
                                text: config.AlertText,
                                time: "5000",
                                bkgrndColour: "#ee6363",
                                textColour: "#FFFFFF"
                            });
                            $('#NotificationMessages').append('<li class="notify-msg" data-info="'+config.AlertInfo+'"><a><h6>' + config.AlertText + '</h6></a></li>');
                        }
                    }
                }
            }
            if (config.Config == "snippet") {
                fncDeviceGaugeChart(config.GraphData);
            }
            else if (config.Config == "bargraph") {
                fncDeviceComChart(config.BarGraphvalues, config.BarGraphKeys);
            }
            else if (config.Config == "piechart") {
                fncDevicepieChart(config.GraphData);
            }
            else if (config.Config == "chats") {
                var Data = config.GraphData.OneClickRes;
                var DashBoardID = $('#dashboard-' + config.GraphData.OnClickResultID);
                if (DashBoardID && DashBoardID.length > 0) {
                    $.each(Data, function (index, value) {
                        $('#key-' + config.GraphData.OnClickResultID).html(index);
                        $('#value-' + config.GraphData.OnClickResultID).html(value);
                    });

                }

            }
            //else if (config.Config == "QuoteReportData") {
            //    var id = $('#tData-' + JsonData.ID);
            //    if (id.length > 0) {
            //        fncReportdata(JsonData);
            //    }
            //}
        }
    };
    notifications.client.LeadtraceFlowChat = function (LeadCount, Flag) {
        if (Flag == true) {
            if (LeadCount.length > 0) {
                if ($('#XI-4464').length > 0) {
                    if ($('#XI-4464')[0].innerHTML < parseInt(LeadCount[0].LeadCount)) {
                        $('#ne-btn-21').click();
                    }
                }
                if ($('#XI-4471').length > 0) {
                    if ($('#XI-4471')[0].innerHTML < parseInt(LeadCount[1].LeadCount)) {
                        $('#ne-btn-1').click();
                    }
                }
                if ($('#XI-4470').length > 0) {
                    if ($('#XI-4470')[0].innerHTML < parseInt(LeadCount[2].LeadCount)) {
                        $('#ne-btn-4').click();
                    }
                }
                if ($('#XI-4468').length > 0) {
                    if ($('#XI-4468')[0].innerHTML < parseInt(LeadCount[4].LeadCount)) {
                        $('#ne-btn-14').click();
                    }
                }
                if ($('#XI-4467').length > 0) {
                    if ($('#XI-4467')[0].innerHTML < parseInt(LeadCount[5].LeadCount)) {
                        $('#ne-btn-15').click();
                    }
                }
                if ($('#XI-4470,#XI-4466').length > 0) {
                    if (($('#XI-4470')[0].innerHTML > parseInt(LeadCount[2].LeadCount)) && ($('#XI-4466')[0].innerHTML < parseInt(LeadCount[6].LeadCount))) {
                        $('#ne-btn-19').click();
                    }
                }
                if ($('#XI-4471,#XI-4465').length > 0) {
                    if (($('#XI-4471')[0].innerHTML > parseInt(LeadCount[1].LeadCount)) && ($('#XI-4465')[0].innerHTML < parseInt(LeadCount[7].LeadCount))) {
                        $('#ne-btn-13').click();
                    }
                }
                if ($('#XI-4468,#XI-4465').length > 0) {
                    if (($('#XI-4468')[0].innerHTML > parseInt(LeadCount[4].LeadCount)) && ($('#XI-4465')[0].innerHTML < parseInt(LeadCount[7].LeadCount))) {
                        $('#ne-btn-17').click();
                    }
                }
                if ($('#XI-4468,#XI-4466').length > 0) {
                    if (($('#XI-4468')[0].innerHTML > parseInt(LeadCount[4].LeadCount)) && ($('#XI-4466')[0].innerHTML < parseInt(LeadCount[6].LeadCount))) {
                        $('#ne-btn-16').click();
                    }
                }
                if ($('#XI-4467,#XI-4466').length > 0) {
                    if (($('#XI-4467')[0].innerHTML > parseInt(LeadCount[5].LeadCount)) && ($('#XI-4466')[0].innerHTML < parseInt(LeadCount[6].LeadCount))) {
                        $('#ne-btn-18').click();
                    }
                }
                if ($('#XI-4471,#XI-4466').length > 0) {
                    if (($('#XI-4471')[0].innerHTML > parseInt(LeadCount[1].LeadCount)) && ($('#XI-4466')[0].innerHTML < parseInt(LeadCount[6].LeadCount))) {
                        $('#ne-btn-12').click();
                    }
                }
                if ($('#XI-4469,#XI-4466').length > 0) {
                    if (($('#XI-4469')[0].innerHTML > parseInt(LeadCount[3].LeadCount)) && ($('#XI-4466')[0].innerHTML < parseInt(LeadCount[6].LeadCount))) {
                        $('#ne-btn-8').click();
                    }
                }
                if ($('#XI-4464,#XI-4469').length > 0) {
                    if (($('#XI-4464')[0].innerHTML > parseInt(LeadCount[0].LeadCount)) && ($('#XI-4469')[0].innerHTML < parseInt(LeadCount[3].LeadCount))) {
                        $('#ne-btn-20').click();
                    }
                }
                if ($('#XI-4471,#XI-4469').length > 0) {
                    if (($('#XI-4471')[0].innerHTML > parseInt(LeadCount[1].LeadCount)) && ($('#XI-4469')[0].innerHTML < parseInt(LeadCount[3].LeadCount))) {
                        $('#ne-btn-22').click();
                    }
                }
                if ($('#XI-4470,#XI-4469').length > 0) {
                    if (($('#XI-4470')[0].innerHTML > parseInt(LeadCount[2].LeadCount)) && ($('#XI-4469')[0].innerHTML < parseInt(LeadCount[3].LeadCount))) {
                        $('#ne-btn-23').click();
                    }
                }
            }
        }
        for (i = 0; i < LeadCount.length; i++) {
            var oneClickID = $('#XI-' + LeadCount[i].ID);
            if (oneClickID && oneClickID.length > 0) {
                var PreviousCount = parseInt(oneClickID[0].innerHTML);
                var PresentCount = parseInt(LeadCount[i].LeadCount);
                if (PreviousCount != PresentCount) {
                    oneClickID.html(PresentCount);
                }
            }
        }

    };
    //Heat Map SignalR
    notifications.client.LeadData = function (JsonData, type) {
        //debugger;
        var id = $('#tData-' + JsonData.ID);
        if (id.length > 0) {
            fncReportdata(JsonData, type);
        }
    }

    // Start the connection.
    $.connection.hub.start().done(function () {
    }).fail(function (e) {
        alert(e);
    });
    $("#MessageCountDailog").dialog({
        width: 200, height: 200, modal: true, autoOpen: false,
        buttons:
            {
                Close: function () {
                    $(this).dialog("close");
                }
            }
    });
    function PopupID(userid, oneclick, PopupOrDialog) {
        $.ajax({
            url: GetSignalROneClickContent,
            type: 'POST',
            contentType: "application/json; charset=utf-8",
            //datatype: "html",
            datatype: "json",
            cache: false,
            async: false,
            data: JSON.stringify({ userID: userid, oneclick: oneclick, PopupOrDialog: PopupOrDialog }),
            success: function (data) {
                if (PopupOrDialog.toLowerCase() == "popup") {
                    var Popup = window.open('', '_blank', "scrollbars=1,resizable=1,width=1600, height=500");
                    with (Popup.document) {
                        open();
                        write(data);
                        close();
                    }
                }
                else {
                    var sGUID = CreateGuid();
                    var sContentCode = data;
                    //var DialogOpacityValue = fncGetDialogOpacity(120);
                    var DialogDivID = "ResultDialog-" + sGUID;
                    var windowMaxWidth = '<i class="windowWidth fa fa-arrows-h" title="" onclick="fncdialogchange(this, &quot;maxwidth&quot;)"></i>';
                    var windowMaxHeight = '<i class="windowHeight fa fa-arrows-v" onclick="fncdialogchange(this, &quot;maxheight&quot;)"></i>';
                    var windowMinWidth = '<i class="windowminWidth fa fa-compress" onclick="fncdialogchange(this, &quot;minwidth&quot;)"></i>';
                    var windowMinHeight = '<i class="windowminHeight fa fa-compress" onclick="fncdialogchange(this, &quot;minheight&quot;)"></i>';
                    var MinDia = '<i class="Minimize fa fa-window-minimize" onclick="fncdialogchange(this, &quot;minimize&quot;)"></i>';
                    var MaxDia = '<i class="Maximize fa fa-window-maximize" onclick="fncdialogchange(this, &quot;maximize&quot;)"></i>';
                    var RestoreDia = '<i class="RestoreDown fa fa-window-restore" onclick="fncdialogchange(this, &quot;restore&quot;)"></i>';
                    var windowclose = '<i class="windowClose fa fa-close" onclick="fncdialogclose(this, ' + false + ', &quot;' + DialogDivID + '&quot;)"></i>';
                    var InPopup = '<i class="openinpopup fa fa-arrow-right" onclick="fncOpenInPopup(&quot;' + "" + '&quot;, ' + 0 + ', ' + 0 + ', &quot;' + sGUID + '&quot;)"></i>';
                    //var RefreshPopup = '<i class="refreshpopup fa fa-refresh" onclick="fncRefreshPopup(&quot;' + DialogDivID + '&quot;, &quot;' + sGUID + '&quot;, ' + 0 + ' )"></i>';
                    var sContentHTML = '<div class="LayoutCode_' + sGUID + ' sys-layout" data-guid="' + sGUID + '" data-name="LayoutGUID">' + sContentCode + '</div>';
                    var Div = '<div class="dialog-box ' + DialogDivID + '" title="Confirm Message"><a><span class="ui-button-icon-primary ui-icon ui-icon-closethick"></span></a></div>';
                    $('#MessagePopup').append(Div);
                    $("." + DialogDivID).html(sContentHTML);
                    $("." + DialogDivID).dialog({
                        title: ' ',
                        appendTo: "body",
                        height: screen.height - 190,
                        width: screen.width - 50,
                        resizable: true,
                        IsCloseIcon: true,
                        //dialogClass: DialogOpacityValue,
                        buttons: [
                        ],
                        open: function () {
                            $(this).parent().promise().done(function () {
                                var dlgWidth; var dlgHeight; var dlgTop; var dlgLeft;
                                $(this).children('.ui-dialog-titlebar').children("div.dialogIcons").remove();
                                //$(this).children('.ui-dialog-titlebar').append('<div class="dialogIcons" data-dinfo = "">' + RefreshPopup + InPopup + MaxDia + RestoreDia + windowMaxWidth + windowMinWidth + windowMaxHeight + windowMinHeight + windowclose + '</div>');
                                $(this).children('.ui-dialog-titlebar').append('<div class="dialogIcons" data-dinfo = "">' + InPopup + MaxDia + RestoreDia + windowMaxWidth + windowMinWidth + windowMaxHeight + windowMinHeight + windowclose + '</div>');
                                $(this).children('.ui-dialog-titlebar').children('.dialogIcons').children('i.RestoreDown').hide();
                                $(this).children('.ui-dialog-titlebar').children('.dialogIcons').children('i.windowminWidth').hide();
                                $(this).children('.ui-dialog-titlebar').children('.dialogIcons').children('i.windowminHeight').hide();
                                uidialog = $(this);
                                var dlgStyle = uidialog[0].attributes["style"].textContent;
                                if (dlgStyle && dlgStyle.length > 0) {
                                    var Styles = dlgStyle.split(";");
                                    for (var k = 0; k < Styles.length; k++) {
                                        var Sty = Styles[k].trim();
                                        var st = Sty.split(":");
                                        if (st[0].trim() == "width") {
                                            dlgWidth = st[1].trim();
                                        }
                                        else if (st[0].trim() == "height") {
                                            dlgHeight = screen.height + "px";
                                        }
                                        else if (st[0].trim() == "top") {
                                            dlgTop = st[1].trim();
                                        }
                                        else if (st[0].trim() == "left") {
                                            dlgLeft = st[1].trim();
                                        }
                                    }
                                    $(this).attr('data-dlgWidth', dlgWidth);
                                    $(this).attr('data-dlgHeight', dlgHeight);
                                    $(this).attr('data-dlgTop', dlgTop);
                                    $(this).attr('data-dlgLeft', dlgLeft);
                                }
                            });
                        },
                        close: function (event, ui) {
                            $("." + DialogDivID).parent().hide();
                        }
                    }).dialog("widget")
                        .draggable({
                            containment: [-screen.width + 600, 0, screen.width - 200, screen.height - 200],
                            start: function () {
                                $(this).data("startingScrollTop", $(this).parent().scrollTop());
                                $(this).parent().scrollTop();
                            },
                            drag: function (event, ui) {
                                var st = parseInt($(this).data("startingScrollTop"));
                                ui.position.top -= $(this).parent().scrollTop() - st;
                            }
                        }).css({ position: "fixed" })
                        .dblclick(function () {
                            if ($(this).hasClass("actualScreenToggle")) {
                                $(this).removeClass("actualScreenToggle");
                                $(this).addClass("fullScreenToggle");
                                var maxIcon = $(this).find('i.Maximize');
                                fncdialogchange(maxIcon, "maximize");
                            }
                            else {
                                $(this).removeClass("fullScreenToggle");
                                $(this).addClass("actualScreenToggle");
                                var restoreIcon = $(this).find('i.RestoreDown');
                                fncdialogchange(restoreIcon, "restore");
                            }
                        })
                }
            },
            error: function (error) {
            }
        });
    }

    function SignalRInboxCount() {
        $.ajax({
            url: ReqirementOSCount,
            contentType: 'application/html ; charset:utf-8',
            type: 'GET',
            dataType: 'html'
        });
    }
    function GetNotificationCount() {
        $.ajax({
            url: NotificationCount,
            contentType: 'application/html ; charset:utf-8',
            type: 'GET',
            dataType: 'JSON',
            success: function (Result) {
                //debugger;
                if (Result.length > 0) {
                    $.each(Result, function (key, val) {
                        if (val.UserID == parseInt(UserID) || val.iRoleID == parseInt(RoleID)) {
                        if (val.ShowCount == 30) {
                            $('#' + val.ConstantID).html(val.Count);
                        }
                        else {
                            $('#' + val.ConstantID).html("");
                        }
                        }
                    });
                }
            },
        });
    }
    function GetLeadData() {
        debugger;
        var OneClickIDs = "7670,7669,7668";
        $.ajax({
            url: GetLeadsData,
            contentType: 'application/html ; charset:utf-8',
            data: { ID: OneClickIDs },
            type: 'GET',
            dataType: 'JSON',
            success: function (Result) {

            },
        });
    }

});
