"use strict";

let connection = new signalR.HubConnectionBuilder().withUrl('/timerHub').build();
connection.start().then(() => connection.stream('StreamDate').subscribe({
    next: (date) => {
        document.querySelector(".date").innerHTML = date;
    },
    error: (err) => console.error(err),
    complete: () => { }
})).catch((err) => console.error(err));
