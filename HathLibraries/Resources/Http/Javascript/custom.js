function LoadSubPage(sub, ignoreanim)
{
	var curpage = $(".nav li.active");
	curpage.removeClass("active");
	$(".nav li a[href='#" + sub +"']").parent().addClass("active");
	
	if(ignoreanim)
	{
		$("#" + curpage.children("a").attr("href").substring(1)).hide();
		$("#" + sub).show();
	}
	else
	{
		$("#" + curpage.children("a").attr("href").substring(1)).animate({ opacity: 0 }, 250, function()
		{
			$(this).hide();
			$("#" + sub).show().animate({ opacity: 1 }, 250);
		});
	}
}

function clamp(val, min, max)
{
	if(val >= min && val <= max)
		return val;
		
	if(val > max)
		return max;
		
	if(val < min)
		return min;
}

function csn(val, fix)
{
	return parseFloat(val).toFixed(fix ? fix : 0).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

$(function()
{
	if(window.location.hash.length > 1)
		LoadSubPage(location.href.split("#")[1], true);

	$(".nav li").click(function()
	{
		LoadSubPage($(this).children("a").attr("href").substring(1), false);
	});
	
	var Caps;
	
	$.get("/stats/caps", function(Data)
	{
		Caps = $.parseJSON(Data);
	});
	
	setInterval(function()
	{
		jQuery.ajax(
		{
			url: "/stats/json",
			async: false,
			success: function(Data)
			{
				var obj = $.parseJSON(Data);
				
				$("#HttpRequestsReceivedThisRun").html(csn(obj['HttpRequestsReceivedThisRun']));
				$("#HttpRequestsSentThisRun").html(csn(obj['HttpRequestsSentThisRun']));
				$("#FilesDownloadedThisRun").html(csn(obj['FilesDownloadedThisRun']));
				$("#FilesUploadedThisRun").html(csn(obj['FilesUploadedThisRun']));
				$("#BytesReceivedThisRun").html(csn(obj['BytesReceivedThisRun'] / 1024 / 1024, 3) + " mb");
				$("#BytesSentThisRun").html(csn(obj['BytesSentThisRun'] / 1024 / 1024, 3) + " mb");
				
				$("#TotalHttpRequestsRevceived").html(csn(obj['TotalHttpRequestsRevceived']));
				$("#TotalHttpRequestsSent").html(csn(obj['TotalHttpRequestsSent']));
				$("#TotalFilesDownloaded").html(csn(obj['TotalFilesDownloaded']));
				$("#TotalFilesUploaded").html(csn(obj['TotalFilesUploaded']));
				$("#TotalBytesReceived").html(csn(obj['TotalBytesReceived'] / 1024 / 1024, 3) + " mb");
				$("#TotalBytesSent").html(csn(obj['TotalBytesSent'] / 1024 / 1024, 3) + " mb");
				
				$("#FilesInCache").html(csn(obj['FilesInCache']));
				$("#UsedSpace").html(csn(obj['UsedSpace'] / 1024 / 1024, 3) + " mb");
				$("#UnregisteredFiles").html(csn(obj['UnregisteredFiles']));
				$("#LastKeepAliveSecondsAgo").html(csn(obj['LastKeepAliveSecondsAgo']) + " s");
				$("#RequestsPerSecond").html(csn(obj['RequestsPerSecond']) + " per second");
				
				var pupload 	= obj['BytesSentDelta'] / Caps['UploadMax'] * 100;
				var pdownload 	= obj['BytesReceivedDelta'] / Caps['DownloadMax'] * 100;
				
				var opupload 	= obj['OveralUploadDelta'] / Caps['UploadMax'] * 100;
				var opdownload 	= obj['OveralDownloadDelta'] / Caps['DownloadMax'] * 100;
				
				
				$("#puploada").css({"width": (clamp(pupload, 0, 50)) + "%"});
				$("#pdownloada").css({"width": (clamp(pdownload, 0, 50)) + "%"});
				
				$("#puploadb").css({"width": (clamp(pupload - 50, 0, 30)) + "%"});
				$("#pdownloadb").css({"width": (clamp(pdownload - 50, 0, 30)) + "%"});
				
				$("#puploadc").css({"width": (clamp(pupload - 80, 0, 20)) + "%"});
				$("#pdownloadc").css({"width": (clamp(pdownload - 80, 0, 20)) + "%"});
				
				$("#puploadc").html(csn(obj['BytesSentDelta'] / 1024, 3) + " kb/s"); 
				$("#pdownloadc").html(csn(obj['BytesReceivedDelta'] / 1024, 3) + " kb/s");
				
				
				
				$("#opuploada").css({"width": (clamp(opupload, 0, 50)) + "%"});
				$("#opdownloada").css({"width": (clamp(opdownload, 0, 50)) + "%"});
				
				$("#opuploadb").css({"width": (clamp(opupload - 50, 0, 30)) + "%"});
				$("#opdownloadb").css({"width": (clamp(opdownload - 50, 0, 30)) + "%"});
				
				$("#opuploadc").css({"width": (clamp(opupload - 80, 0, 20)) + "%"});
				$("#opdownloadc").css({"width": (clamp(opdownload - 80, 0, 20)) + "%"});
				
				$("#opuploadc").html(csn(obj['OveralUploadDelta'] / 1024, 3) + " kb/s"); 
				$("#opdownloadc").html(csn(obj['OveralDownloadDelta'] / 1024, 3) + " kb/s");
			}
		});
	}, 1000);
});