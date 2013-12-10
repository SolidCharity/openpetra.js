function OpenTab(name, title)
{
    if ($("#TabbedWindows:has(#tab" + name + ")").length == 0)
    {
        $("#TabbedWindows").append("<li class='tab' id='tab" + name + "'><a href='#'>" +
            // could use <span class='glyphicon glyphicon-remove'></span> instead of x?
            "<button class='close closeTab' type='button' id='btnClose" + name + "'>x</button>" + title + "</a>" +
            "</li>");
        $("#tab" + name).click(function() { ActivateTab(name); });
        
        $("#wndHome").parent().append("<div class='OpenPetraWindow' id='wnd" + name + "'></div>");
        
        // TODO fetch screen content from the server
        screenContent = "<h1>" + title + "</h1>";
        if (name == "Partner_Partners")
        {
            screenContent = "<h3>Maintain Partners</h3>";
            screenContent += "<a href='javascript:OpenTab(\"frmPartnerFind\", \"Partner Find\")'>Find Partners</a><br/>";
            screenContent += "<a href='javascript:OpenTab(\"frmPartnerEdit\", \"Partner Edit\")'>Work with Last Partner</a><br/>";
            screenContent += "<a href='#'>Delete Partner</a><br/>";
            screenContent += "<h3>Create Partner</h3>";
            screenContent += "<a href='#'>Add new Family</a><br/>";
            screenContent += "<a href='#'>Add new Person</a><br/>";
            screenContent += "<a href='#'>Add new Organisation</a><br/>";
            screenContent += "<a href='#'>Add new Bank</a><br/>";
            screenContent += "<a href='#'>Add new Venue</a><br/>";
            screenContent += "<a href='#'>Add new Unit</a><br/>";
            $("#wnd" + name).html(screenContent);
        } 
        else if (name.substring(0, "frm".length) === "frm")
        {
            $("#wnd" + name).load("/lib/loadform.aspx?form=" + name);
        }

        $("#btnClose" + name).click(function(e) 
        {
            e.preventDefault();
            $("#tab"+name).hide();
            ActivateTab("Home");
            return false;
        });
    }
    ActivateTab(name);
};

function ActivateTab(name)
{
    $(".nav-tabs .tab").removeClass("active");
    $(".OpenPetraWindow").hide();
    $("#tab" + name).show();
    $("#tab" + name).addClass("active");
    $("#wnd" + name).show();
//    $('html, body').animate({
//                        scrollTop: $("#TabbedWindows").offset().top + $("#topnavigation").height
//                    }, 1);
}

function AddMenuGroup(name, title, menuitems)
{
    $("#LeftNavigation").append("<a href='#' class='list-group-item' id='mnuGrp" + name + "'>" + title + "</a><ul class='nav' id='mnuLst" + name + "'></ul>");
    menuitems(name);
    $("#mnuGrp" + name).click(function() {
        if (!$(this).hasClass('active'))
        {
            $(".list-group-item").removeClass("active");
            $(".list-group .nav ").hide();
            $(this).addClass("active");
        }
        $(this).next().toggle();
    });
}

function AddMenuItem(parent, name, title, tabtitle)
{
    $("#mnuLst" + parent).append("<li><a href='#' id='" + name + "'>" + title + "</a></li>");
    $("#" + name).click(function() {OpenTab(this.id, tabtitle);});
}

jQuery(document).ready(function() {
    $("#logout").click(function() {
      $.ajax({
          type: "POST",
          url: "/server.asmx/Logout",
          data: "{}",
          contentType: "application/json; charset=utf-8",
          dataType: "json",
          success: function(data, status, response) {
            result = JSON.parse(response.responseText);
            if (result['d'] == true)
            {
                // alert("Successful logged out");
                window.location = "Default.aspx";
            }
            else
            {
                alert("could not log out");
            }
          },
          error: function(response, status, error) {
            alert("Error: could not log out");
          },
          fail: function(msg) {
            alert("Fail: could not log out");
          }
        });      
    });

    $("#tabHome").click(function() { ActivateTab("Home"); });
});
