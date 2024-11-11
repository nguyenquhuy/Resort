// layer
$(document).ready(function(){
	$('.gallery-grid .item').mouseenter(function(){
		$(this).find('.info-layer').css('animation-name','infoShow');
	});
	$('.gallery-grid .item').mouseleave(function(){
		$(this).find('.info-layer').css('animation-name','infoHide');
	});
});


// button
$(document).ready(function(){
	$('.gallery-grid .item').mouseenter(function(){
		$(this).find('.layer-item').css('animation-name','buttonShow');
	});
	$('.gallery-grid .item').mouseleave(function(){
		$(this).find('.layer-item').css('animation-name','buttonHide');
	});
});

//set background for each image
$(document).ready(function(){
	$('.gallery-grid .image').each(function(){
		$bgImg = $(this).children('img').attr('src');
		$(this).css('background-image','url('+ $bgImg +')');
	});
});


//image popup - show and hide
$(document).ready(function () {
    // When the zoom icon is clicked
    $('.info-layer .zoom').click(function () {
        // Get the src of the image to be zoomed
        let bgImg = $(this).closest('.item').find('.bg-replace').attr('src');

        // Set the src of the image in the popup
        $('.image-show img').attr('src', bgImg);

        // Display the popup
        $('.image-show').css('display', 'block');
    });

    // Close button functionality
    $('.image-show .close-popup').click(function () {
        // Hide the popup
        $('.image-show').css('display', 'none');
    });
});

$('.info-layer .zoom').click(function () {
    alert("Zoom icon clicked!");
});


// close image popup when not click on it
$(document).mouseup(function(e){
    if (!$('.image-item img').is(e.target) && $('.image-item img').has(e.target).length === 0){
        $('.image-show').hide();
    }
});

