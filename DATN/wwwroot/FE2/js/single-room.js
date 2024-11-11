$(".slide-image").slick({
  slidesToShow: 1,
  slidesToScroll: 1,
  arrows: true,
  //   asNavFor: ".slide-list",
});
$(".slide-list").slick({
  slidesToShow: 4,
  slidesToScroll: 1,
  asNavFor: ".slide-image",
  centerMode: true,
  focusOnSelect: true,
});

$("#input-rooms").mousemove(function () {
  let a = $("#input-rooms").val();
  $("#price").html(a);
});

$("#input-rooms").click(function () {
  let a = $("#input-rooms").val();
  $("#price").html(a);
});

// rating

$(function () {
  $("#accommodation").barrating({
    theme: "fontawesome-stars",
  });
  $("#meals").barrating({
    theme: "fontawesome-stars",
  });
  $("#transport").barrating({
    theme: "fontawesome-stars",
  });
  $("#destination").barrating({
    theme: "fontawesome-stars",
  });
  $("#overall").barrating({
    theme: "fontawesome-stars",
  });
});
