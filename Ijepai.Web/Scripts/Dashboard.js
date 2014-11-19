Grids["QuickCreateGrid"] = {
    classPrefix: "QC",
    url: "/Dashboard/GetQC",
    id: "ID",
    columns: {
        Name: { title: "VM Name" },
        Machine_Size: { title: "Machine Size" },
        OS: { title: "OS", style: "width: 185px" },
        RecepientEmail: { title: "Email Address", style: "width: 185px" },
        Status: { title: "Status", style: "width: 100px" },
        QCActions: { title: "Actions"}
    },
    actions: {
        QC_act: {
            policy: "Disallow",
            title: "Extend Lab Duration",
            method: function (id) {
                alert("id");
            }
        }
    },
    subgrid: false
}
$(function () {
    Grid.Init("#QC-list","QuickCreateGrid")
})