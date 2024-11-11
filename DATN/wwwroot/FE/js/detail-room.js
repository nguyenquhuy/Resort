// zoom layer on image
$(document).ready(function(){
	$('.left .room-img').mouseenter(function(){
		$(this).find('.zoom-layer').css('animation-name','layerShow');
		$(this).find('.zoom-btn').css('animation-name','btnShowUp');
	})
	$('.left .room-img').mouseleave(function(){
		$(this).find('.zoom-layer').css('animation-name','layerHide');
		$(this).find('.zoom-btn').css('animation-name','btnHideDown');
	})
});


// calendar
$(document).ready(function(){
	$('#calendar-init').datepicker({
		todayHighlight: true,
	});
});

//image popup - show and hide
$(document).ready(function(){
	$bgImg = $('.room-img .bg-replace').attr('src');
	$('.room-img').css('background-image','url('+ $bgImg +')');
	$('.zoom-layer .zoom-btn').click(function(){
		$('.img-box img').attr('src',$bgImg);
		$('.img-popup').css('display','block');
	});
	$('.img-popup .close-popup').click(function(){
		$('.room-img .img-popup').css('display','none');
	});
});

// close image popup when not click on it
$(document).mouseup(function(e) 
{
    // if the target of the click isn't the container nor a descendant of the container
    if (!$('.img-box img').is(e.target) && $('.img-box img').has(e.target).length === 0){
        $('.img-popup').hide();
    }
});