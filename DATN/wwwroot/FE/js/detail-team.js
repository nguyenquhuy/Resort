// image zoom
$(document).ready(function(){
	$('.member-detail .member-img').mouseenter(function(){
		$(this).find('.zoom-layer').css('animation-name','layerShow');
		$(this).find('.zoom-btn').css('animation-name','btnShowUp');
	})
	$('.member-detail .member-img').mouseleave(function(){
		$(this).find('.zoom-layer').css('animation-name','layerHide');
		$(this).find('.zoom-btn').css('animation-name','btnHideDown');
	})
});


// skill count up
$(document).ready(function(){
  $('.number-count').counterUp({
    time: 2000,
  });
});

//image popup - show and hide
$(document).ready(function(){
	$bgImg = $('.member-img .bg-replace').attr('src');
	$('.member-img').css('background-image','url('+ $bgImg +')');
	$('.zoom-layer .zoom-btn').click(function(){
		$('.img-box img').attr('src',$bgImg);
		$('.img-popup').show();
		$('.img-popup img').css('animation-name','zoomOut');
	});
	$('.img-popup .close-popup').click(function(){
		$('.img-popup img').css('animation-name','zoomIn');
		$('.member-img .img-popup').hide();
	});
});

// close image popup when not click on it
$(document).mouseup(function(e) 
{
    // if the target of the click isn't the container nor a descendant of the container
    if (!$('.img-box img').is(e.target) && $('.img-box img').has(e.target).length === 0){
    	$('.img-popup img').css('animation-name','zoomIn');
        $('.img-popup').hide();
    }
});