// menu toggle desktop -------------------------------------->
$(document).ready(function () {
  $(".menu-main li").hover(
    function () {
      $(this).find("ul:first").slideDown(500);
      $(this).find("ul:first").css("display", "flex");
    },
    function () {
      $(this).find("ul:first").hide(600);
    }
  );
});
// menu toggle mobile and tablet -------------------------------------->
$(document).ready(function () {
  $("#span-1").click(function () {
    $(".span-1").toggle(500);
  });
  $("#span-2").click(function () {
    $(".span-2").toggle(500);
  });
  $("#span-3").click(function () {
    $(".span-3").toggle(500);
  });
  $("#span-4").click(function () {
    $(".span-4").toggle(500);
  });
  $("#span-5").click(function () {
    $(".span-5").toggle(500);
  });
  $("#span-6").click(function () {
    $(".span-6").toggle(500);
  });
  $("#span-7").click(function () {
    $(".span-7").toggle(500);
  });
});

// scroll page -------------------------------------->
$(document).ready(function () {
  $(window).scroll(function (event) {
    let pos_body = $("html,body").scrollTop();

    // change color menu desktop
    if (pos_body > 30) {
      // change logo
      $("#logo-home-1").css("display", "none");
      $("#logo-home-2").css("display", "block");
      // change color of menu
      $(".scroll").css("color", "#000");
      // change color of header
      $(".header").css("background-color", "#fff");
      // change color icon search, shopping, menu contact
      $(".header__menu-icon svg").css("color", "#000");
      // change color and background of span
      $(".shopping-bag-group span").css({
        "background-color": "#ffb400",
        color: "#fff",
      });
      $(".header__nav svg").css("color", "#000");
    } else {
      //change logo
      $("#logo-home-1").css("display", "block");
      $("#logo-home-2").css("display", "none");
      // change color of menu
      $(".scroll").css("color", "#fff");
      // change color of header
      $(".header").css("background-color", "#ffb400");
      // change color icon search, shopping, menu contact
      $(".header__menu-icon svg").css("color", "#fff");
      // change color and background of span
      $(".shopping-bag-group span").css({
        "background-color": "#fff",
        color: "#000",
      });
      $(".header__nav svg").css("color", "#fff");
    }
  });
});

// show header contact -------------------------------------->
$(document).ready(function () {
  $("#show-contact-menu").click(function () {
    $(".header__contact").css({ right: "0", visibility: "visible" });
  });
  $(".header__contact-close").click(function () {
    $(".header__contact").css({ right: "-405px", visibility: "hidden" });
  });
});

// validate -------------------------------------->

$(document).ready(function () {
  // for email home page
  $(".footer__search").validate({
    rules: {
      email: {
        required: true,
        email: true,
      },
    },
    messages: {
      email: {
        required: "Please enter email",
        email: "Email not true",
      },
    },
  });

  // for contact home page
  $("#form-contact").validate({
    rules: {
      name: "required",
      email: {
        required: true,
        email: true,
      },
    },
    messages: {
      name: "Please enter name",
      email: {
        required: "Please enter email",
        email: "Email not true",
      },
    },
  });

  // for search in 404 page
  $(".page404__search").validate({
    rules: {
      page404: "required",
    },
    messages: {
      page404: "Please enter something...",
    },
  });
});

// show menu tablet and mobile -------------------------------------->
$(document).ready(function () {
  $(".header__nav").click(function () {
    if ($(".header__menu-mobile").css("left") == "-2000px") {
      $(".header__menu-mobile").css("left", "0");
    } else {
      $(".header__menu-mobile").css("left", "-2000px");
    }
  });
});

// slick for testimonial -------------------------------------->
$(".testimonial").slick({
  dots: true,
  infinite: false,
  speed: 300,
  slidesToShow: 3,
  slidesToScroll: 1,
  arrows: false,
  responsive: [
    {
      breakpoint: 1024,
      settings: {
        slidesToShow: 2,
        slidesToScroll: 1,
        infinite: true,
        dots: true,
      },
    },
    {
      // to 0 up 768 show 1 slide
      breakpoint: 769,
      settings: {
        slidesToShow: 1,
        slidesToScroll: 1,
      },
    },
  ],
});
// slick for banner

$(".bannerSlider").slick({
  speed: 300,
  slidesToShow: 1,
  slidesToScroll: 1,
  arrows: true,
});

// slick for testimonial
$(".testimonials-slick").slick({
  infinite: true,
  speed: 300,
  slidesToShow: 4,
  slidesToScroll: 1,
  autoplay: true,
  arrows: false,
  autoplaySpeed: 2000,
  responsive: [
    {
      breakpoint: 1024,
      settings: {
        slidesToShow: 3,
        slidesToScroll: 1,
        infinite: true,
      },
    },
    {
      breakpoint: 769,
      settings: {
        slidesToShow: 2,
        slidesToScroll: 1,
      },
    },
    {
      breakpoint: 480,
      settings: {
        slidesToShow: 1,
        slidesToScroll: 1,
      },
    },
  ],
});

// slide for icon slick
$(".iconSlick").slick({
  infinite: true,
  slidesToShow: 5,
  slidesToScroll: 1,
  autoplay: true,
  autoplaySpeed: 2000,
  responsive: [
    {
      breakpoint: 1024,
      settings: {
        slidesToShow: 3,
        slidesToScroll: 1,
        infinite: true,
      },
    },
    {
      breakpoint: 769,
      settings: {
        slidesToShow: 2,
        slidesToScroll: 1,
      },
    },
    {
      breakpoint: 480,
      settings: {
        slidesToShow: 1,
        slidesToScroll: 1,
      },
    },
  ],
});

// masonry -------------------------------------->
$(function () {
  var $grid = $(".masonry-grid").masonry({});
  // layout Masonry after each image loads
  $grid.imagesLoaded().progress(function () {
    $grid.masonry("layout");
  });
});

//  daterangepicker -------------------------------------->

$(function () {
  $('input[name="checkIn"]').daterangepicker({
    singleDatePicker: true,
    startDate: new Date(),
    showDropdowns: true,
    timePicker: true,
    timePicker24Hour: true,
    timePickerIncrement: 10,
    autoUpdateInput: true,
    locale: {
      format: "DD/MM/YYYY",
    },
  });
});

// toggle comment in blog detail page
$("#reply-01").click(function () {
  $("#for-reply-01").toggle(300);
});
$("#reply-02").click(function () {
  $("#for-reply-01").toggle(300);
});

$("#reply-03").click(function () {
  $("#for-reply-02").toggle(300);
});
$("#reply-04").click(function () {
  $("#for-reply-02").toggle(300);
});
