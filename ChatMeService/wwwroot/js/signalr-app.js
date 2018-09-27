String.prototype.replaceAll = function (search, replacement) {
    var target = this;
    return target.replace(new RegExp(search, 'g'), replacement);
};

//(function(){
var messageBox = "\
<div class='panel panel-default'>\
    <div class='panel-body'>\
        {message}\
    </div>\
    <div class='panel-footer'>\
        <small>{time}</small>\
        <span class='pull-right'>{sender}</span>\
    </div>\
</div>";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.start().finally(() => {
    if (connection.connection.connectionState == 1) {
        connection.invoke("GetFriendsList", 1, 50).catch(err => console.error(err.toString()));
        connection.invoke("GetConversations", 1, 50).catch(err => console.error(err.toString()));
    }
}).catch(err => console.error(err.toString()));

connection.on("FriendsList", (users) => {
    users.forEach(user => {
        $("#FriendsList").append($(parseFriend(user)));
    });
});

connection.on("Conversations", (conversations) => {
    conversations.forEach(conversation => {
        $("#ConversationsList").append($(parseConversation(conversations)));
    });
});

connection.on("ReceivePosts", (posts) => {
    posts.forEach(post => {
        $("#Timeline").append($(parsePost(post)));
    });
});

connection.on("ReceivePost", (post) => {
    $("#Timeline").prepend($(parsePost(post)));
});

$("#Send").click(function () {
    const message = document.getElementById("Message").value;

    connection.invoke("Publish", message).catch(err => console.error(err.toString()));

    $("#Message").val("");

    event.preventDefault();
});

$("#Message").keyup(function (e) {
    // TODO: Check if shift not holded
    if (e.keyCode == 13) {
        $("#Send").click();
    }
});

function parseDateTime(dateTime) {
    var d = new Date(dateTime);

    var r = d.getFullYear() + "/"
        + _2(d.getMonth() + 1) + "/"
        + _2(d.getDay()) + " - "
        + _2(d.getHours()) + ":"
        + _2(d.getMinutes()) + ":"
        + _2(d.getSeconds());

    return r;
}

function _2(n) {
    if (n > 10)
        return n;
    else
        return "0" + n;
}

function parsePost(post) {
    var msg = messageBox;
    msg = msg.replaceAll("{time}", parseDateTime(post.addDateTime));
    msg = msg.replaceAll("{message}", post.message);
    msg = msg.replaceAll("{sender}", post.owner.userName);

    return msg;
}
//})();