var Grid = {
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
        Grid.Load(el, conf);
    },
    GridMethods : {
        populateParticipantTable: function (rowId, conf, Data) {
            var row = $("#" + rowId);
            var labId = rowId.slice(4);
            if (Data.length != 0) {
                $("#" + rowId + "-subgrid-row .no-data-found").css("display", "none");
                var config = Grids[conf];
                var tbody = $("<tbody />");
                var subgridColumns = config.subgridColumns;
                var index = 0;
                var status = row.find(".Status").html();
                var participantActions = {
                    editParticipant: {
                        policy: "Allow",
                        title: "Edit participant particulars."
                    },
                    moveParticipant: {
                        policy: "Allow",
                        title: "Move or copy participant to another lab."
                    },
                    getMachineLink: {
                        policy: "Allow",
                        title: "Get participant machine's link."
                    },
                    deleteParticipant: {
                        policy: "Disallow",
                        title: "Remove this participant from lab."
                    }
                }
                for (i in Data) {
                    index++;
                    var participantRow = $("<tr />").attr("id", "participant-" + Data[i].ID);
                    var vmControl = $("<td class='vm-status glyphicon glyphicon-play' title='Click to start the VM'>");
                    vmControl.click(function () {
                        if ($(this).hasClass("glyphicon-play")) {
                            $(this).removeClass("glyphicon-play").addClass("glyphicon-stop").attr("title", "Click to stop the VM");
                        } else {
                            $(this).removeClass("glyphicon-stop").addClass("glyphicon-play").attr("title", "Click to start the VM");
                        }
                    });
                    participantRow.append(vmControl).append("<td>" + index + "</td>");
                    var actionNode = $("#participant-actions-template");
                    $.each(actionNode.find("li"), function () {
                        $(this).attr("title", participantActions[$(this).data("role")].title);
                        if (participantActions[$(this).data("role")].policy == "Disallow") {
                            $(this).addClass("inactiv-action").attr("title", "The action is not appropriate in current context");
                        }
                    });
                    Data[i]["participantActions"] = actionNode.html();
                    for (cell in subgridColumns) {
                        participantRow.append("<td class='" + cell + "'>" + Data[i][cell] + "</td>");
                    }
                    participantRow.find(".paricipant-edit").not(".inactive-action").click(function (e) {
                        e.preventDefault();
                        row = $(this).parent().parent().parent();
                        $("#Username").val(row.find(".Email_Address").html());
                        $("#First_Name").val(row.find(".First_Name").html());
                        $("#Last_Name").val(row.find(".Last_Name").html());
                        $("#Role").val(row.find(".Role").html());
                        $("#edit-participant-lab-id").val(labId);
                        $("#edit-participant-id").val(row.attr("id").slice(12));
                        $("#overlay").fadeIn();
                        $("#edit-participant-form-container").fadeIn();
                        return false;
                    });
                    participantRow.find(".participant-move").not(".inactive-action").click(function (e) {
                        e.preventDefault();
                        row = $(this).parent().parent().parent();
                        $("#move-participant-id").val(row.attr("id").slice(12));
                        $("#move-participant-lab-id").val(labId);
                        $("#overlay").fadeIn();
                        $("#move-participant-form-container").fadeIn();
                        return false;
                    });
                    participantRow.find(".participant-machine-link").not(".inactive-action").click(function (e) {
                        e.preventDefault();
                        row = $(this).parent().parent().parent();
                        $("#lab-id-machine-link").val(labId);
                        $("#participant-id-machine-link").val(row.attr("id").slice(12));
                        $("#overlay").fadeIn();
                        $("#get-lab-link-form-container").fadeIn();
                        return false;
                    });
                    participantRow.find(".participant-delete").not(".inactive-action").click(function (e) {
                        e.preventDefault();
                        row = $(this).parent().parent().parent();
                        $("#lab-id-delete-participant").val(labId);
                        $("#participant-id-delete-participant").val(row.attr("id").slice(12))
                        $("#overlay").fadeIn();
                        $("#delete-participant-form-container").fadeIn();
                        return false;
                    })
                    tbody.append(participantRow);
                }
                if ($("#" + rowId + "-subgrid-row").find("tbody").length) {
                    $("#" + rowId + "-subgrid-row .subgrid-table tbody").replaceWith(tbody);
                } else {
                    $("#" + rowId + "-subgrid-row .subgrid-table").append(tbody);
                }
                $("#" + rowId + "-subgrid-row .subgrid-table").fadeIn();
            } else {
                $("#" + rowId + "-subgrid-row .no-data-found").fadeIn();
                $("#" + rowId + "-subgrid-row .subgrid-table").css("display", "none");
            }
            AjaxLoader.hide("#" + rowId + " .expander", "glyphicon-minus");
            row.addClass("loaded").addClass("open-subgrid");
            $("#" + rowId + "-subgrid-row").css("display", "table-row");
        },
        populateEditParticipantList: function (rowId, conf, Data) {
            $("#lab-participants-edit").css("display", "block");
            $("#edit-participants-form-list").html("");
            var index = 0;
            if (Data.length == 0) {
                NewLabForm.create_new_participant_row({ Role: "Admin", Index: index }, "edit-participants-form-list");
                index++;
            } else {
                for (i in Data) {
                    NewLabForm.create_new_participant_row({
                        Index: index,
                        Email_Address: Data[i]["Email_Address"],
                        First_Name: Data[i]["First_Name"],
                        Last_Name: Data[i]["Last_Name"],
                        Role: Data[i]["Role"]
                    }, "edit-participants-form-list");
                    index++;
                }
            }
            $("#edit-lab-participants-add-participant").attr("count", index);
        },
        populateEditLabParticipantList: function (rowId, conf, Data) {
            $("#create-lab-participant-list").html("");
            var index = 0;
            if (Data.length == 0) {
                NewLabForm.create_new_participant_row({ Role: "Admin", Index: index }, "create-lab-participant-list");
                index++;
            } else {
                for (i in Data) {
                    NewLabForm.create_new_participant_row({
                        Index: index,
                        Email_Address: Data[i]["Email_Address"],
                        First_Name: Data[i]["First_Name"],
                        Last_Name: Data[i]["Last_Name"],
                        Role: Data[i]["Role"]
                    }, "create-lab-participant-list");
                    index++;
                }
            }
            $("#create-lab-form-add-participant").attr("count", index);
        }
    },
    LoadSubgrid: function (rowId, conf, fn) {
        fn = Grid.GridMethods[fn] ? Grid.GridMethods[fn] : Grid.GridMethods["populateParticipantTable"];
        $.ajax({
            url: "/Labs/GetLabParticipants",
            type: "POST",
            data: { id: rowId.slice(4) },
            success: function (data, status, xhr) {
                if (data.Status == "0") {
                    var Data = data.rows[0];
                    fn(rowId, conf, Data);
                } else {
                    alert("Some error occured")
                }
                $("#" + rowId + "-subgrid-row .subgrid-data").fadeTo(500, 1);
                $("#" + rowId + "-subgrid-row .load-screen").css("display", "none");
            }
        })
    },
    LoadSubGridRow: function(list){

    },
    LoadGridRow: function(list){

    },
    Load: function (el, conf) {
        var config = Grids[conf];
        var labActions = config.labActions;
        $.ajax({
            url: config.url,
            type: "POST",
            success: function (data, status, xhr) {
                var totalItems = data.TotalItems;
                var rows = data.rows;
                if (data.Status == "0") {
                    if (rows.length != 0) {
                        var cols = config.columns;
                        var colNum = 7;
                        var classes = ["odd", "even"];
                        var hasSubgrid = config.subgrid;
                        var tbody = $("<tbody />");
                        var tr = "";
                        var serialNum = 0;
                        for (i in rows) {
                            serialNum = Number(i) + 1;
                            tr = $("<tr />").attr("id", "lab-" + rows[i][config.id]);
                            if (hasSubgrid) {
                                tr.append("<td class=\"expander collapsed-lab-row glyphicon glyphicon-plus\" />");
                            }
                            tr.append("<td>" + serialNum + "</td>").addClass("lab-row " + classes[i % 2]);
                            var actionsNode = $("#lab-actions-template").clone();
                            rows[i]["VM_Count"] = rows[i]["Participant_Count"] + " / " + rows[i]["VM_Count"];
                            var status = rows[i]["Status"];
                            if (status == "Scheduled") {
                                labActions.deleteLab.policy = "Permit";
                                labActions.editParticipant.policy = "Permit";
                                labActions.editLab.policy = "Permit";
                                labActions.refreshParticipantList.policy = "Permit";
                            } else if (status == "Starting") {
                                labActions.extendLab = "Permit";
                                labActions.editParticipant = "Permit";
                                labActions.refreshParticipantList.policy = "Permit";
                            } else if (status == "Available") {
                                labActions.extendLab = "Permit";
                                labActions.editParticipant = "Permit";
                                labActions.deleteLab = "Permit";
                                labActions.refreshParticipantList.policy = "Permit";
                            } else if (status == "Stopping") {
                                labActions.refreshParticipantList.policy = "Permit";
                            } else if (status == "Exhausted") {
                                labActions.deleteLab.policy = "Permit";
                                labActions.refreshParticipantList.policy = "Permit";
                            }
                            $.each(actionsNode.find("li"), function () {
                                var id = rows[i].ID;
                                var actionOb = labActions[$(this).data("role")];
                                $(this).attr("title", actionOb.title);
                                if (actionOb.policy == "Disallow"){
                                    $(this).addClass("inactive-action").attr("title", "The action is not appropriate in current context");
                                } else {
                                    $(this).click(function () {
                                        actionOb.method(id);
                                    });
                                }
                            });
                            rows[i]["labActions"] = actionsNode.children();
                            for (field in cols) {
                                tr.append($("<td>").addClass(field).append(rows[i][field]));
                            }
                            tbody.append(tr);
                            if (hasSubgrid) {
                                var subgridCols = config.subgridColumns;
                                var subgridClassPrefix = config.subgridClassPrefix;
                                var row = $("<tr id=\"lab-" + rows[i][config.id] + "-subgrid-row\" class=\"subgrid-container-row\" />");
                                var subGridMarker = $("<td class=\"subgrid-marker\"></td>");
                                var subGridCell = $("<td class='subgrid-data' colspan=\"" + colNum + "\"></td>");
                                var subGridContainer = $("<div class=\"carved-box\" />");
                                var loadScreen = $("<div class='load-screen' />");
                                var subGrid = $("<table id=\"" + rows[i][config.id] + "-subgrid\" class=\"subgrid-table\" />");
                                var head = $("<thead />")
                                var headings = $("<tr />");
                                headings.append("<th class='vm-status'></th>").append($("<th class=\"participant-index\">S No</th>"))
                                for (j in subgridCols) {
                                    headings.append($("<th style=\"" + subgridCols[j].style + "\">" + subgridCols[j].title + "</th>"))
                                }
                                tbody.append(row.append(subGridMarker).append(subGridCell.append(subGridContainer.append(loadScreen).append(subGrid.append(head.append(headings))).append("<p class=\"no-data-found\">No participants are associated with this lab.</p>"))));
                            }
                        }
                        if ($(el).find("tbody").length) {
                            $(el).find("tbody").replaceWith(tbody)
                        } else {
                            $(el).append(tbody)
                        }
                        $.each($(el + " .expander"), function () {
                            $(this).click(function () {
                                var row = $(this).parent("tr");
                                var loadedEarlier = row.hasClass("loaded");
                                if ($(this).hasClass("glyphicon-plus")) {
                                    if (!loadedEarlier ) {
                                        AjaxLoader.show("#" + row.attr("id") + " .expander");
                                        Grid.LoadSubgrid(row.attr("id"), conf);
                                    } else {
                                        $(this).addClass("glyphicon-minus").removeClass("glyphicon-plus");
                                        row.addClass("loaded").addClass("open-subgrid");
                                        $("#" + row.attr("id") + "-subgrid-row").css("display", "table-row");
                                    }
                                } else {
                                    $(this).removeClass("glyphicon-minus").addClass("glyphicon-plus");
                                    row.addClass("loaded").removeClass("open-subgrid");
                                    $("#" + row.attr("id") + "-subgrid-row").css("display", "none");
                                }
                            })
                        })
                    } else {
                        alert("No data");
                    }
                } else {
                    alert("An error occured");
                }
            }
        })
    },
}
var Grids = {
    LabGrid: {
        classPrefix: "lab",
        url: "/Labs",
        id: "ID",
        columns: {
            Name: { title: "Lab Name" },
            Time_Zone: { title: "Time Zone" },
            Start_Time: { title: "Start Time", style: "width: 185px" },
            End_Time: { title: "End Time", style: "width: 185px" },
            Status: { title: "Status", style: "width: 100px" },
            VM_Count: { title: "Total VMs" },
            Networked: { title: "Networked" },
            OS: { title: "OS" },
            VM_Type: { title: "VM Type" },
            Hard_Disk: { title: "Machine Size" },
            labActions: { title: "Actions" }
        },
        labActions: {
            extendLab: {
                policy: "Disallow",
                title: "Extend Lab Duration",
                method: function(id){
                    $("#overlay").fadeIn();
                    var row = $("#lab-" + id);
                    $("#reschedule-lab-start-time").val(row.find(".Start_Time").html());
                    $("#reschedule-lab-end-time").val(row.find(".End_Time").html());
                    $("#reschedule-lab-id").val(id);
                    $("#extend-lab-form-content").fadeIn();
                }
            },
            deleteLab: {
                policy: "Disallow",
                title: "Remove Lab With Assets",
                method: function (id) {
                    $("#delete-lab-id").val(id );
                    $("#overlay").fadeIn();
                    $("#delete-lab-form-content").fadeIn();
                }
            },
            editLab: {
                policy: "Disallow",
                title: "Edit Lab Particulars",
                method: function(id){
                    Grid.LoadSubgrid("lab-" + id, "LabGrid", "populateEditLabParticipantList");

                    $("#create-lab-form").trigger("reset");
                    var row = $("#lab-" + id);
                    $("#Lab_ID_for_edit").val(id);
                    $("#Name").val(row.find(".Name").html());
                    $("#Time_Zone").val(row.find(".Time_Zone").html());
                    $("#Start_Time").val(row.find(".Start_Time").html());
                    $("#End_Time").val(row.find(".End_Time").html());
                    $("#VM_Count").val(row.find(".VM_Count").html().split(" / ")[1]);
                    $("#VM_Type").val(row.find(".VM_Type").html());
                    $("#OS").val(row.find(".OS").html());
                    $("#Machine_Size").val(row.find(".Hard_Disk").html());
                    $("#Networked").val(row.find(".Networked").html());

                    $("#overlay").toggle();
                    $("#create-lab-form-content").fadeIn(function () {
                        $("#create-lab-form-tabs li.active-lab-form-tab").removeClass("active-lab-form-tab");
                        $("#show-create-lab-form-tab").addClass("active-lab-form-tab");
                    });
                }
            },
            editParticipant: {
                policy: "Disallow",
                title: "Add or Remove Participants",
                method: function (id) {
                    $("#overlay").fadeIn();
                    $("#edit-participants-form-content").fadeIn();
                    $("#edit-participants-lab-id").val(id);
                    Grid.LoadSubgrid("lab-" + id, "LabGrid", "populateEditParticipantList");
                }
            },
            refreshParticipantList: {
                policy: "Disallow",
                title: "Refresh participant list of this Lab",
                method: function(id){
                    var rowId = "#lab" + id;
                    $(rowId + "-subgrid-row .load-screen").css("display", "block");
                    AjaxLoader.show(rowId + " .expander");
                    $(rowId + "-subgrid-row .subgrid-data").fadeTo(500, 0.3);
                    Grid.LoadSubgrid("lab-" + id, conf);
                }
            }
        },
        subgrid: true,
        subgridClassPrefix: "participant",
        subgridColumns: {
            Email_Address: { title: "Username" },
            First_Name: { title: "First Name" },
            Last_Name: { title: "Last Name" },
            Role: { title: "Role"},
            participantActions: { title: "Actions" }
        }
    }
}
var Message = {
    visible: 0,
    timer: null,
    defaultIndicator: "glyphicon-bell",
    easeOutBounce: function (x, t, b, c, d) {
        if ((t /= d) < (1 / 2.75)) {
            return c * (7.5625 * t * t) + b;
        } else if (t < (2 / 2.75)) {
            return c * (7.5625 * (t -= (1.5 / 2.75)) * t + .75) + b;
        } else if (t < (2.5 / 2.75)) {
            return c * (7.5625 * (t -= (2.25 / 2.75)) * t + .9375) + b;
        } else {
            return c * (7.5625 * (t -= (2.625 / 2.75)) * t + .984375) + b;
        }
    },
    easeInExpo: function (x, t, b, c, d) {
        return (t == 0) ? b : c * Math.pow(2, 10 * (t / d - 1)) + b;
    },
    show: function (heading, content, indicator, color, stay) {
        if (Message.visible == 0) {
            $("#message-type-indicator").css("color", color);
            if (indicator !== false) $("#message-type-indicator").removeClassRegEx(/^glyphicon-/).addClass(indicator);
            if (heading !== false) $("#message-heading").html(heading);
            if (heading !== false) $("#message-content").html(content);
            $.easing.showMessage = Message.easeOutBounce;

            $("#message").animate({
                opacity: 1,
                right: 0
            }, 1500, "showMessage")
            if (stay !== true) {
                Message.timer = setTimeout(function () { Message.hide(indicator); }, 5000)
            }
            Message.visible = 1;
        } else {
            Message.put(heading, content, indicator, color, stay)
        }
    },
    hide: function (indicator) {
        $.easing.hideMessage = Message.easeInExpo;

        $("#message").animate({
            opacity: 0,
            right: "-35%"
        }, 1000, "hideMessage", function () {
            $("#message-type-indicator").removeClass(indicator).addClass(Message.defaultIndicator);
            //$("#message-heading").html("");
            //$("#message-content").html("");
        })
        Message.visible = 0;
    },
    put: function (heading, content, indicator, color, stay) {
        clearTimeout(Message.timer)
        if (Message.visible == 0) {
            Message.show(heading, content, indicator, color, stay)
        } else {
            if (heading !== false) $("#message-heading").html(heading);
            if (content !== false) $("#message-content").html(content);
            if (stay !== true) {
                Message.timer = setTimeout(Message.hide, 5000);
            }
        }
    },
    dissolve: function () {
        clearTimeout(Message.timer);
        $("#message").fadeOut(300, function () {
            $(this).css({ right: "-35%", display: "block", opacity: 0 });
        });
        Message.visible = 0;
    }
}
var AjaxLoader = {
    timer: {},
    show: function (selector) {
        AjaxLoader.timer[selector] = {};
        AjaxLoader.timer[selector].class = $(selector).classRegEx("glyphicon-")
        $(selector).addClass("ajax-loader").removeClass(AjaxLoader.timer[selector].class);
        AjaxLoader.timer[selector].handler = setInterval(function () {
            $(selector).css("background-position", function (i, value) {
                var x = parseInt(value, 10);
                if (x <= -494) {
                    x = 0;
                } else {
                    x -= 26;
                }
                return x + "px" + " 0";
            })
        }, 50);
    },
    hide: function (selector, selectorClass) {
        if (selectorClass === null) selectorClass = "glyphicon-hand-right";
        if(selectorClass === true) selectorClass = AjaxLoader.timer[selector].class
        $(selector).removeClass("ajax-loader").addClass(selectorClass);
        clearInterval(AjaxLoader.timer[selector].handler);
    }
}
var Notification = {
    Init: function () {
        $("#notification-btn").click(function () {
            $("#notification-wrap").slideToggle();
        })
    },
    Show: function () {
        $("#notification-wrap").slideDown();
        Notification.Visible = 1;
    },
    Put: function (message, title) {
        var node = $("#notification-template").clone();
        node.find(".notification-message").html(message);
        $("#notification").append(node.html()).attr("title", title);
        $(".close-notification-row").click(function () {
            var totalNotifications = $("#notifications").find(".notification-row").length;
            var title = "";
            $(this).parent().fadeOut(function () {
                $(this).remove();
                if (totalNotifications == 0) {
                    title = "No new notification.";
                    $("#no-notification-notice").show();
                    setTimeout(Notification.Hide, 2000);
                } else if (totalNotifications == 1) {
                    title = "1 Notification."
                } else {
                    title = totalNotifications + " Notifications."
                }
                $("#notification-btn").attr("title", title)
            });
        });
        if (!Notification.Visible) {
            Notification.Show();
            $("#no-notification-notice").css("display", "none");
        }

        $("#no-notification-notice").hide();
        if ($("#notifications").find(".notification-row").length == 1) {
            $("#notification-btn").attr("title", "1 Notification")
        } else {
            $("#notification-btn").attr("title", $("#notifications").find(".notification-row").length + " Notifications")
        }
    },
    Hide: function () {
        $("#notification-wrap").slideUp();
        Notification.Visible = 0;
    }
}
var Lib = {
}
var AppData = {
    LoggedIn: 0,
    Load: function () {
        App.getLabList();
        $("#close-edit-participant-form").click(App.closeEditParticipantForm);
        $("#clear-participant-particulars").trigger("reset");
        $("#close-move-participant-form").click(App.closeMoveParticipantForm);
        $("#close-delete-participant-form").click(App.closeDeleteParticipantForm);
        $("#cancel-participant-deletion").click(App.closeDeleteParticipantForm);
        $("#close-get-machine-link-form").click(App.closeGetMachineLinkForm);
        $("#cancel-get-machine-link").click(App.closeGetMachineLinkForm);
        Grid.Init("#lab-list", "LabGrid");
        $("#Start_Time").datetimepicker({
            format: 'd/M/Y H:i'
        });
        $("#End_Time").datetimepicker({
            format: 'd/M/Y H:i'
        });
        $("#reschedule-lab-start-time").datetimepicker({
            format: 'd/M/Y H:i'
        });
        $("#reschedule-lab-end-time").datetimepicker({
            format: 'd/M/Y H:i'
        });
    }
}
App = {
    Init: function () {
        App.setPage();
        App.setNav("top-nav");
    },
    LabList: [],
    getLabList: function () {
        $.ajax({
            url: "/Labs/LabList",
            dataType: "json",
            type: "post",
            success: function (data, status, xhr) {
                if (data.Status == 0) {
                    var rows = data.rows;
                    App.LabList = rows;
                    var selectBox = $("<select name='newLab_ID'>").addClass("form-control").attr("id", "move-to-lab-options");
                    for (i in rows) {
                        selectBox.append("<option value='" + rows[i].ID + "'>" + rows[i].Name + "</option>");
                    }
                    $("#target-lab-container").html(selectBox);
                }
            }
        })
    },
    closeEditParticipantForm: function(){
        $("#edit-participant-form-container").fadeOut(function () {
            $("#edit-participant-form").trigger("reset");
            $("#overlay").fadeOut(200);
        })
        return false;
    },
    closeMoveParticipantForm: function(){
        $("#move-participant-form-container").fadeOut(function () {
            $("#move-participant-form").trigger("reset");
            $("#overlay").fadeOut(200);
        })
        return false;
    },
    closeDeleteParticipantForm: function(){
        $("#delete-participant-form-container").fadeOut(function () {
            $("#delete-participant-form").trigger("reset");
            $("#overlay").fadeOut(200);
        })
        return false;
    },
    closeGetMachineLinkForm: function(){
        $("#get-lab-link-form-container").fadeOut(function () {
            $("#get-machine-link-form").trigger("reset");
            $("#overlay").fadeOut();
        })
        return false;
    },
    editParticipantFormSuccess: function(data){
        if (data.Status == 0) {
            Grid.LoadSubgrid(data.Lab, "LabGrid");
            App.closeEditParticipantForm();
        }
    },
    UpdateVMStatus: function (data) {
        $.ajax({
            url: "/Dashboard/GetVMStatus",
            type: "POST",
            data: { id: rowId.slice(4) },
            success: function (data, status, xhr) {
                if (data.Status == "0") {
                    var Data = data.rows[0];
                    fn(rowId, conf, Data);
                } else {
                    alert("Some error occured")
                }
                $("#" + rowId + "-subgrid-row .subgrid-data").fadeTo(500, 1);
                $("#" + rowId + "-subgrid-row .load-screen").css("display", "none");
            }
        })
    },
    moveParticipantFormSuccess: function(data){
        if (data.Status == 0) {
            Grid.LoadSubgrid(data.prevLab, "LabGrid");
            Grid.LoadSubgrid(data.newLab, "LabGrid");
            App.closeMoveParticipantForm();
        }
    },
    deleteParticipantFormSuccess: function(data){
        if (data.Status == 0) {
            AjaxLoader.show("#" + data.Lab + " .expander");
            $("#" + data.Lab + "-subgrid-row .load-screen")
            $("#" + data.Lab + "-subgrid-row .subgrid-data").fadeTo(500, 0.3);
            Grid.LoadSubgrid(data.Lab, "LabGrid");
            App.closeDeleteParticipantForm();
        }
    },
    getMachineLinkFormSuccess: function(){

    },
    showTab: function (el) {
        var tab = $("#" + el);
        var prevTab = $(".selected-tab").first();
        prevTab.removeClass("selected-tab").css({ left: "5000px", opacity: 0.3 });
        tab.addClass("selected-tab").css({ left: 0, opacity: 1 });
    },
    setNav: function (el) {
        $("#" + el + " a").each(function () {
            $(this).click(function (e) {
                e.preventDefault();
                if ($(this)[0] !== $(".selected-tab-btn")[0]) {
                    $(".selected-tab-btn").removeClass("selected-tab-btn");
                    $(this).addClass("selected-tab-btn");
                    App.showTab($(this).attr("id").slice(0, -4));
                }
                return false;
            })
        });
    },
    setPage: function () {
        $("#content").height($(window).height() - ($("#header").height() + $("#footer").height()));
    },
    LoadPages: function () {
        $.ajax({
            url: "/Home/Templates",
            type: "post",
            success: function (response, status, xhr) {
                $("#content").append(response);
            }
        });
        $.ajax({
            url: "/Dashboard/GetView",
            type: "post",
            success: function (response, status, xhr) {
                $("#dashboard-tab").append(response);
            }
        });
        $.ajax({
            url: "/Labs/GetView",
            type: "post",
            success: function (response, status, xhr) {
                $("#lab-tab").append(response);
            }
        });
        $.ajax({
            url: "/Statistics/GetView",
            type: "post",
            success: function (response, status, xhr) {
                $("#stats-tab").append(response);
            }
        });
        $.ajax({
            url: "/Bills/GetView",
            type: "post",
            success: function (response, status, xhr) {
                $("#bill-tab").append(response);
            }
        });
    }
}
var NewLabForm = {
    fd: {},
    Init: function () {
        $("#create-lab-form-tabs li").click(function () {
            if ($(this).hasClass("active-lab-form-tab")) {
                return false;
            }
            $("#create-lab-form-body > div").fadeOut(0);
            $("#" + $(this).attr("id").slice(5)).fadeIn(300);
            $("#create-lab-form-tabs li.active-lab-form-tab").removeClass("active-lab-form-tab");
            $(this).addClass("active-lab-form-tab");
        });
        $("#reset-lab-btn").click(function (e) {
            $("#create-lab-form").trigger("reset")
        });
        $("#create-lab-form-close").click(NewLabForm.closeCreateLabForm);
        $("#create-lab-form-add-participant").click(function (e) {
            e.preventDefault();
            count = Number($(this).attr("count"));
            NewLabForm.create_new_participant_row({ Index: count });
            $(this).attr("count", ++count);
            return false;
        });
        $("#edit-lab-participants-add-participant").click(function (e) {
            e.preventDefault();
            count = Number($(this).attr("count"));
            NewLabForm.create_new_participant_row({ Index: count }, "edit-participants-form-list");
            $(this).attr("count", ++count);
            return false;
        })
        $(".toggle-machine-size-os").click(function () {
            $(".predefined-size-os").fadeToggle(300);
            $(".choose-custom-machine").fadeToggle(300);
        });
        $("#vm-image").change(function () {
            if ($(this).val() !== "") {
                NewLabForm.fd[$(this).attr("name")] = $(this)[0].files[0];
            }
        });
        $("#close-participants-form").click(function (e) {
            e.preventDefault();
            NewLabForm.closeEditParticipantForm();
            return false;
        });
        $("#close-extend-lab-form").click(function (e) {
            e.preventDefault();
            NewLabForm.closeRescheduleLabForm();
            return false;
        });
        $("#cancel-lab-deletion-btn").click(function (e) {
            e.preventDefault();
            NewLabForm.closeDeleteLabForm();
            return false;
        });
        $("#close-delete-lab-form").click(function (e) {
            e.preventDefault();
            NewLabForm.closeDeleteLabForm();
            return false;
        });
        (function uploadDataDisk() {
            $(".data-disk").unbind("change");
            $(".data-disk").change(function () {
                if ($(this).val() !== "") {
                    NewLabForm.fd[$(this).attr("name")] = $(this)[0].files[0];
                    var index = $(".data-disk-list").children().length;
                    if ($(".data-disk:last-child").val() !== "") {
                        $(".data-disk-list").append("<input type='file' name='data-disk-" + index + "' class='data-disk' style='margin-top:5px' />");
                    }
                    uploadDataDisk();
                }
            })
        })();
        (function uploadCustomSoftware() {
            $(".custom-software").unbind("change");
            $(".custom-software").change(function () {
                if ($(this).val() !== "") {
                    NewLabForm.fd[$(this).attr("name")] = $(this)[0].files[0];
                    var index = $(".custom-software-list").children().length;
                    if ($(".custom-software:last-child").val !== "") {
                        $(".custom-software-list").append("<input type='file' name='custom-software-" + index + "' class='custom-software' style='margin-top:5px' />");
                    }
                }
            })
        })();   
        NewLabForm.send = function(name) {
            var formData = new FormData();
            var file = NewLabForm.fd[name];
            var fileId = file.name.split(".")[0] + "_";
            formData.append("dataFile", file);

            var notice = $("#upload-file-notice-template").clone();
            var title = "Uploading : '" + file.name + "',   Size : " + file.size;
            notice.find(".upload-file-progress-bar-core").addClass(fileId)
            notice.find(".upload-filename").html(file.name).click(function () {
                if ($(this).css("text-overflow") == "elipsis") {
                    $(this).css({ "text-overflow": "", "word-wrap": "break-word" });
                } else {
                    $(this).css({ "text-overflow": "elipsis", "word-wrap": "normal" });
                }
            });
            notice.find(".upload-filesize").html(file.size).click(function () {
                if ($(this).css("text-overflow") == "elipsis") {
                    $(this).css("text-overflow", "");
                } else {
                    $(this).css("text-overflow", "");
                }
            });
            Notification.Put(notice.html(), title);

            var xhr = new XMLHttpRequest();
            xhr.open("POST", "/Labs/UploadLabResources", true);
            xhr.upload.addEventListener("progress", function (evt) {
                alert(evt.loaded)
                var percentageComplete = Math.round((evt.loaded * 100) / evt.total);
                $("." + fileId).find("upload-file-progress-bar-core").css("width", percentageComplete + "%")
            }, false);
            xhr.addEventListener("load", function (evt) { NewLabForm.UploadComplete(evt); }, false);
            xhr.addEventListener("error", function (evt) { NewLabForm.UploadFailed(evt); }, false);
            xhr.send(formData);
        }

        NewLabForm.UploadComplete = function(evt) {
            if (evt.target.status == 200)
                alert("File uploaded successfully.");
            else
                alert("Error Uploading File");
        }

        NewLabForm.UploadFailed = function(evt) {
            alert("There was an error attempting to upload the file.");

        }
    },
    closeDeleteLabForm: function () {
        Grid.Load("#lab-list", "LabGrid");
        $("#delete-lab-form-content").fadeOut(function () {
            $("#overlay").fadeOut(200);
            $("#delete-lab-form").trigger("reset");
        })
    },
    deleteLabFormSuccess: function () {
        App.getLabList();
        NewLabForm.closeDeleteLabForm();
    },
    closeRescheduleLabForm: function () {
        Grid.Load("#lab-list", "LabGrid");
        $("#extend-lab-form-content").fadeOut(function () {
            $("#overlay").fadeOut(200);
            $("#reschedule-lab-form").trigger("reset");
        })
    },
    rescheduleLabFormSuccess: function(){
        NewLabForm.closeRescheduleLabForm();
    },
    closeEditParticipantForm: function(){
        $("#edit-participants-form-content").fadeOut(function () {
            $("#overlay").fadeOut(200);
        })
    },
    editParticipantFormSuccess: function (data) {
        AjaxLoader.show("#" + data.Lab + " .expander");
        $("#" + data.Lab + "-subgrid-row .load-screen")
        $("#" + data.Lab + "-subgrid-row .subgrid-data").fadeTo(500, 0.3);
        NewLabForm.closeEditParticipantForm();
        Grid.LoadSubgrid(data.Lab, "LabGrid")
    },
    closeCreateLabForm: function () {
        $("#create-lab-form-content").fadeOut(function () {
            $("#overlay").fadeOut(200);
            $("#create-lab-form-body > div").fadeOut(0);
            $("#create-lab-form-tab").fadeIn(0);
            $("#create-lab-form").trigger("reset");
            $("#create-lab-form-tabs li.active-lab-form-tab").removeClass("active-lab-form-tab");
            $("#show-create-lab-form-tab").addClass("active-lab-form-tab");
            $("#create-lab-form-add-participant").attr("count", 0);
            $("#create-lab-participant-list").html("");
        });
    },
    createLabFormSuccess: function (data) {
        Grid.Load("#lab-list", "LabGrid");
        NewLabForm.closeCreateLabForm();
        for (i in NewLabForm.fd) {
            NewLabForm.send(i)
        }
        App.getLabList();
        NewLabForm.fd = {};
    },
    create_new_participant_row: function (options, container) {
        container = container ? container : "create-lab-participant-list";
        var participantIndex = options.Index;
        var Username = (options.Email_Address) ? options.Email_Address : "";
        var First_Name = (options.First_Name) ? options.First_Name : "";
        var Last_Name = (options.Last_Name) ? options.Last_Name : "";
        var Selected = (options.Role) ? options.Role : "Guest";
        var delBtn = $("<a/>").attr("class", "delete-participant-row").attr("href", "#").html("Delete");
        delBtn.css({ position: "absolute", right: "10px", top: "5px", color: "#3399ff" });
        delBtn.click(function (e) {
            $(this).parent().remove();
            var index = 0;
            if (container == "edit-participants-form-list") {
                $("#edit-lab-participants-add-participant").attr("count", Number($("#edit-lab-participants-add-participant").attr("count")) - 1);
            } else {
                $("#create-lab-form-add-participant").attr("count", Number($("#create-lab-form-add-participant").attr("count")) - 1);
            }
            $("#" + container + " .participant-row").each(function () {
                var newParticipantId = "LabParticipants[" + index + "].";
                var fields = { username: "Username", role: "Role", "first-name": "First_Name", "last-name": "Last_Name" }
                var row = $(this);
                for (field in fields) {
                    row.find("." + field + "-label").attr("for", newParticipantId + fields[field])
                    row.find("." + field).attr("id", newParticipantId + fields[field]).attr("name", newParticipantId + fields[field])
                }
                row.children(".new-participant-row-index").html(index + 1);
                index++;

            })
            $("#add-new-participant-row").attr("count", index)
        })

        if (Username == "") {
            var newParticipantRow = $("<div/>", { class: "form-horizontal new-participant-row participant-row" });
        } else {
            var newParticipantRow = $("<div/>", { class: "form-horizontal participant-row" });
        }
        var rowIndex = $("<span/>", { class: "new-participant-row-index" });
        rowIndex.html(participantIndex + 1)
        var firstRow = $("<div/>", { class: "add-participant-row-1" });
        var secondRow = $("<div/>", { class: "add-participant-row-2" });
        var participantId = "LabParticipants[" + participantIndex + "].";

        var usernameContainer = $("<div/>", { class: "form-group col-md-6" })
        var usernameLabel = $("<label>", { class: "control-label col-md-3 username-label", for: participantId + "Username", html: "Username" });
        var usernameInputContainer = $("<div/>", { class: "col-md-9" });
        var usernameInput = $("<input/>", {
            id: participantId + "Username",
            type: "text",
            class: "form-control username",
            name: participantId + "Username",
            value: Username,
            placeholder: "Email Id of user",
            "data-val-required": "Username of participant must be provided",
            "data-val-email": "Username must be Email address."
        })

        var roleContainer = $("<div/>", { class: "form-group col-md-6" });
        var roleLabel = $("<label/>", { class: "control-label col-md-3 role-label", for: participantId + "Role", html: "Role" });
        var roleInputContainer = $("<div/>", { class: "col-md-9" });
        var roleInput = $("<select/>", { class: "form-control role", id: participantId + "Role", name: participantId + "Role" });
        var roleOptions = {
            "Guest": $("<option/>", { value: "Guest", html: "Guest" }),
            "Admin": $("<option/>", { value: "Admin", html: "Admin" })
        }

        var firstNameContainer = $("<div/>", { class: "form-group col-md-6" });
        var firstNameLabel = $("<label/>", { class: "control-label col-md-3 first-name-label", for: participantId + "First_Name", html: "First name" });
        var firstNameInputContainer = $("<div/>", { class: "col-md-9" });
        var firstNameInput = $("<input/>", { class: "form-control first-name", id: participantId + "First_Name", name: participantId + "First_Name", value: First_Name })

        var lastNameContainer = $("<div/>", { class: "form-group col-md-6" });
        var lastNameLabel = $("<label/>", { class: "control-label col-md-3 last-name-label", for: participantId + "Last_Name", html: "Last name" });
        var lastNameInputContainer = $("<div/>", { class: "col-md-9" });
        var lastNameInput = $("<input/>", { class: "form-control last-name", id: participantId + "Last_Name", name: participantId + "Last_Name", value: Last_Name })

        newParticipantRow.append(rowIndex);
        newParticipantRow.append(firstRow);
        newParticipantRow.append(secondRow);
        newParticipantRow.append(delBtn);

        firstRow.append(usernameContainer);
        usernameContainer.append(usernameLabel);
        usernameContainer.append(usernameInputContainer);
        usernameInputContainer.append(usernameInput);

        firstRow.append(roleContainer);
        roleContainer.append(roleLabel);
        roleContainer.append(roleInputContainer);
        roleInputContainer.append(roleInput);
        for (role in roleOptions) {
            roleInput.append(roleOptions[role])
        }

        secondRow.append(firstNameContainer);
        firstNameContainer.append(firstNameLabel);
        firstNameContainer.append(firstNameInputContainer);
        firstNameInputContainer.append(firstNameInput);
        roleOptions[Selected].attr("selected", "");

        secondRow.append(lastNameContainer);
        lastNameContainer.append(lastNameLabel);
        lastNameContainer.append(lastNameInputContainer);
        lastNameInputContainer.append(lastNameInput)

        $("#" + container).append(newParticipantRow)
    }
}
$(function () {
    $(document.body).css("display", "block");
    App.Init();
    if (AppData.LoggedIn == 1) {
        AppData.XS = $("#logout-form input[name='__RequestVerificationToken']").val();
        $.ajaxSetup({
            data: { __RequestVerificationToken: AppData.XS },
            type: "post",
            datatype: "json"
        })
        $("#logout-button").click(function (e) {
            e.preventDefault();
            Message.put("Logging out of Ijepai", "", "glyphicon-hand-right", "", false);
            $("#logout-form").trigger("submit");
            return false;
        })
        $("#new-lab-btn").click(function (e) {
            e.preventDefault();
            $("#Lab_ID_for_edit").val(0);
            if (Number($("#create-lab-form-add-participant").attr("count")) <= 0) {
                NewLabForm.create_new_participant_row({ Index: 0, Role: "Admin" })
                $("#create-lab-form-add-participant").attr("count", 1)
            }
            $("#overlay").toggle();
            $("#create-lab-form-content").fadeIn(function () {
                $("#create-lab-form").trigger("reset");
                $("#create-lab-form-tabs li.active-lab-form-tab").removeClass("active-lab-form-tab");
                $("#show-create-lab-form-tab").addClass("active-lab-form-tab");
            });
            return false;
        })
        $("#reload-lab").click(function (e) {
            e.preventDefault();
            Grid.Load("#lab-list", "LabGrid");
            return false;
        })
        NewLabForm.Init();
        Notification.Init();
        AppData.Load();
    }
    //Message.show(false, false, "glyphicon-info-sign", false, false)
    $(window).resize(App.setPage);
})
function getToken() {
    $.ajax({
        url: "/Account/getToken",
        success: function (data, status, xhr) {
            AppData.new = $(data).find("input[name='__RequestVerificationToken']").val()
        }
    })
}

function LoginSuccess(response) {
    if (response.Status == 0) {
        window.location.href = "/";
    } else {

    }
}

function LoginBegin() {
    Message.put("Requesting log in", "", "glyphicon-hand-right", "", true);
    AjaxLoader.show("#message-type-indicator");
}

function LoginComplete(response) {
    response = $.parseJSON(response.responseText)
    Message.put(response.MessageTitle, response.MessageBody, "", "", false);
    AjaxLoader.hide("#message-type-indicator");
}

function LogoutSuccess(response) {
    window.location.href = "/Account/Login";
}

function LogoutFailure(response) {
    alert(response)
}

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
    $.fn.upload = function (remote, data, successFn, progressFn) {
        // if we dont have post data, move it along
        if (typeof data != "object") {
            progressFn = successFn;
            successFn = data;
        }
        return this.each(function () {
            if ($(this)[0].files[0]) {
                var formData = new FormData();
                formData.append($(this).attr("name"), $(this)[0].files[0]);

                // if we have post data too
                if (typeof data == "object") {
                    for (var i in data) {
                        formData.append(i, data[i]);
                    }
                }

                // do the ajax request
                $.ajax({
                    url: remote,
                    type: 'POST',
                    xhr: function () {
                        myXhr = $.ajaxSettings.xhr();
                        if (myXhr.upload && progressFn) {
                            myXhr.upload.addEventListener('progress', function (prog) {
                                var value = ~~((prog.loaded / prog.total) * 100);

                                // if we passed a progress function
                                if (progressFn && typeof progressFn == "function") {
                                    progressFn(prog, value);

                                    // if we passed a progress element
                                } else if (progressFn) {
                                    $(progressFn).val(value);
                                }
                            }, false);
                        }
                        return myXhr;
                    },
                    data: formData,
                    dataType: "json",
                    cache: false,
                    contentType: false,
                    processData: false,
                    complete: function (res) {
                        var json;
                        try {
                            json = JSON.parse(res.responseText);
                        } catch (e) {
                            json = res.responseText;
                        }
                        if (successFn) successFn(json);
                    }
                });
            }
        });
    };
})(jQuery);