$(document).ready(function(){
	$('.manage-team .item').mouseenter(function(){
		$(this).find('.individual-contact').css('animation-name','contactShowUp');
		$(this).find('.individual-contact .icon').css('animation-name','iconShowUp');
	})
	$('.manage-team .manage-name').mouseenter(function(){
		$(this).parent().find('.individual-contact').css('animation-name','contactShowUp');
		$(this).parent().find('.individual-contact .icon').css('animation-name','iconShowUp');
	})
	$('.manage-team .item').mouseleave(function(){
		$(this).find('.individual-contact').css('animation-name','contactShowDown');
		$(this).find('.individual-contact .icon').css('animation-name','iconShowDown');
	})
	$('.manage-team .manage-name').mouseleave(function(){
		$(this).parent().find('.individual-contact').css('animation-name','contactShowDown');
		$(this).parent().find('.individual-contact .icon').css('animation-name','iconShowDown');
	})
});