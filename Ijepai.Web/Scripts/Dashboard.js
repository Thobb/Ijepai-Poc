var GridQC = {
    openGrids: "",
    Init: function (el, conf) {
        var config = Grids[conf];
        var index = 0;
        var cols = config.columns;
        var prefix = config.classPrefix;
        var headings = $("<tr />");
        if (config.subgrid) {
            headings.append("<th style=\"width: 30px\" />");
        }
        headings.append("<th class=\"table-index " + prefix + "-col-" + index + "\">S No</th>");
        for (i in cols) {
            index++;
            headings.append("<th class=\"" + i + "\" style=\"" + cols[i].style + "\">" + cols[i].title + "</th>");
        }
        $(el).append($("<thead />").append(headings))
        GridQC.Load(el, conf);
    },
    GridMethods: {
        populateQCTable: function (rowId, conf, Data) {
            var row = $("#" + rowId);
            var labId = rowId.slice(4);
            if (Data.length != 0) {
                $("#" + rowId + "-subgrid-row .no-data-found").css("display", "none");
                var tbody = $("<tbody />");
                var subgridColumns = config.subgridColumns;
                var index = 0;
                var status = row.find(".Status").html();
                for (i in Data) {
                    index++;
                    var participantRow = $("<tr />").attr("id", "participant-" + Data[i].ID);
                    var vmControl = $("<td class='vm-status glyphicon glyphicon-play' title='Click to start the VM'>");
                    vmControl.click(function () {
                        thisBtn = $(this);
                        if ($(this).hasClass("glyphicon-play")) {
                            $.ajax({
                                url: "/labs/StartMachine",
                                data: "Participant_ID=" + $(this).parent().attr("id").slice(12) + "&Lab_ID=" + labId,
                                success: function () {
                                    thisBtn.removeClass("glyphicon-play").addClass("glyphicon-stop").attr("title", "Click to stop the VM");
                                }
                            })
                        } else {
                            $.ajax({
                                url: "/labs/StopMachine",
                                data: "Participant_ID=" + $(this).parent().attr("id").slice(12) + "&Lab_ID=" + labId,
                                success: function () {
                                    thisBtn.removeClass("glyphicon-stop").addClass("glyphicon-play").attr("title", "Click to start the VM");
                                }
                            })
                        }
                    });

                    tbody.append(participantRow);
                }

                $("#" + rowId + "-subgrid-row .subgrid-table").fadeIn();
            } else {
                $("#" + rowId + "-subgrid-row .no-data-found").fadeIn();
                $("#" + rowId + "-subgrid-row .subgrid-table").css("display", "none");
            }
        }       
    }
}
    
var GridsQC = {
    LabGridQC: {
        classPrefix: "Dashboard",
        url: "/Dashboard",
        id: "ID",
        columns: {
            Name: { title: "VM Name" },
            OS: { title: "OS" },
            VM_Type: { title: "VM Type" },
            Hard_Disk: { title: "Machine Size" },
            State: { title: "State" },
        }       
    }
}

var AppDataQC = {
    LoggedIn: 0,
    Load: function () {
        AppQC.getQCLabList();
        Grid.Init("#QC-list", "LabGridQC");
    }
}
var AppQC = {
    Init: function () {
        AppQC.setPage();
        
    },
    LabList: [],
    getQCLabList: function () {
        $.ajax({
            url: "/Dashboard/QCList",
            dataType: "json",
            type: "post",
            success: function (data, status, xhr) {
                if (data.Status == 0) {
                    var rows = data.rows;
                    AppQC.LabList = rows;
                }
            }
        })
    },
    setPage: function () {
        $("#content").height($(window).height() - ($("#header").height() + $("#footer").height()));
    },
    UpdateVMStatus: function (data) {
        $("#vmCreateStatus").html(data.Status);
        var statusTimerHandle = setInterval(function () {
            $.ajax({
                url: "/Dashboard/GetVMStatus",
                type: "POST",
                data: "ServiceName=" + data.ServiceName + "&VMName=" + data.VMName,
                success: function (data, status, xhr) {
                    if (data.Status == "0") {
                        $("#vmCreateStatus").html("<b>Machine state : <b>" + data.InstanceStatus + "<br><b>Power state : </b>" + data.PowerState);
                    } else {
                        alert("Some error occured")
                    }
                    if ((data.InstanceStatus == "ReadyRole") && (data.PowerState == "Started")) { //data.Message
                        clearInterval(statusTimerHandle)
                    }
                }
            })
        }, 10000);
    }
};
$(function () {
    $(document.body).css("display", "block");
    AppQC.Init();
    AppDataQC.Load();
    //Message.show(false, false, "glyphicon-info-sign", false, false)
    $(window).resize(App.setPage);
})
(function ($) {
    $.fn.classRegEx = function (regex) {
        var classes = $(this).attr("class");
        if (!classes || !regex) return "";
        classes = classes.split(' ');
        var len = classes.length;
        for (var i = 0; i < len; i++) {
            if (classes[i].match(regex)) return classes[i];
        }
        return "";
    };
    $.fn.hasClassRegEx = function (regex) {
        var classes = $(this).attr('class');
        if (!classes || !regex) return false;
        classes = classes.split(' ');
        var len = classes.length;
        for (var i = 0; i < len ; i++)
            if (classes[i].match(regex)) return true;
        return false;
    };
    $.fn.removeClassRegEx = function (regex) {
        var classes = $(this).attr('class');
        if (!classes || !regex) return false;
        var classArray = [];
        classes = classes.split(' ');
        for (var i = 0, len = classes.length; i < len; i++) if (!classes[i].match(regex)) classArray.push(classes[i]);
        $(this).attr('class', classArray.join(' '));
        return $(this);
    };
}) (jQuery);