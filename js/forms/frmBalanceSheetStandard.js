  // Period Range
$("input[name=periodrange]").change(function() {
    $('#divPeriod :input').attr('disabled', $(this).attr('value') != "period");
    $('#divQuarter :input').attr('disabled', $(this).attr('value') != "quarter");
    $('#divDate :input').attr('disabled', $(this).attr('value') != "date");
});

$(document).ready(function() {
    $("input[value=period]").click();
});