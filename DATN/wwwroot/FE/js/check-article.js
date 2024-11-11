$(document).ready(function(){
	$('.portfolio .item').mouseenter(function(){
		$(this).find('.info-layer').css('animation-name','infoShow');
	});
	$('.portfolio .item').mouseleave(function(){
		$(this).find('.info-layer').css('animation-name','infoHide');
	});
});