const express = require("express");
const cors = require("cors");

// App setup
const port = process.env.PORT || 5000
const app = express();

app.use(cors());

const server = app.listen(port, function () {
  console.log(`Listening on port ${port}`);
});

// Socket setup
const io = require("socket.io")(server, {
  cors:
  {
    "origin": "https://data.virtualbrainlab.org",
    "methods": "GET,HEAD,PUT,PATCH,POST,DELETE",
    "preflightContinue": false,
    "optionsSuccessStatus": 204
  } 
});

ID2Socket = {}; // keeps track of all sockets with the same ID
Socket2ID = {}; // keeps track of the ID of each socket
Socket2Type = {};

reserved_messages = ['connection','disconnect','ID','ReceiveCameraImgMeta','ReceiveCameraImg','log','log-warning','log-error']

io.on("connection", function (socket) {
  console.log("Client connected with ID: " + socket.id);

  socket.on('disconnect', () => {
  	console.log('Client disconnected with ID: ' + socket.id);

  	if (ID2Socket[Socket2ID[socket.id]]) {
      ID2Socket[Socket2ID[socket.id]].splice(ID2Socket[Socket2ID[socket.id]].indexOf(socket.id),1);
      Socket2ID[socket.id] = undefined;
  	}
  })

  socket.on('ID', function(clientData) {
    // ID is just a unique identifier, it can be any string
    newClientID = clientData[0]
    // Type can be "send" or "receive"
    newClientType = clientData[1]


  	console.log('Client ' + socket.id + ' requested to update ID to ' + newClientID);
  	// Check if we have an old clientID that needs to be removed
  	if (Socket2ID[socket.id]) {
  		oldClientID = Socket2ID[socket.id]

  		ID2Socket[oldClientID].splice(ID2Socket[oldClientID].indexOf(socket.id),1);
  	}

  	if (ID2Socket[newClientID] == undefined) {
  		// save the new entry into a new list
  		ID2Socket[newClientID] = [socket.id];
  	}
  	else {
  		// save the new entry
  		ID2Socket[newClientID].push(socket.id);
  	}
  	// update the client ID locally
	  Socket2ID[socket.id] = newClientID;
    Socket2Type[socket.id] = newClientType
  	console.log('User updated their ID to: ' + Socket2ID[socket.id] + " type " + newClientType );
  	console.log('All connected clients with ID: ' + ID2Socket[Socket2ID[socket.id]]);
  });


  // Make sure that these receive events are listed in the reserved_messages list
  
  // Camera receive events
  socket.on('ReceiveCameraImgMeta', function(data) {
    emitToSender(socket.id, 'ReceiveCameraImgMeta', data);
  });
  socket.on('ReceiveCameraImg', function(data) {
    emitToSender(socket.id, 'ReceiveCameraImg', data);
  });
  
  // Receiver events
  socket.on('log', function(data) {
    emitToSender(socket.id, 'log', data);
  });
  socket.on('log-warning', function(data) {
    emitToSender(socket.id, 'log-warning', data);
  });
  socket.on('log-error', function(data) {
    emitToSender(socket.id, 'log-error', data);
  });

  // For all remaining events, asssume they are a sender -> receiver broadcast and emit them automatically
  socket.onAny((eventName, data) => {
    if (!reserved_messages.includes(eventName)) {
      emitToReceiver(socket.id, eventName, data);
    }
  });

});

function emitToReceiver(id, event, data) {
  	console.log('Sender sent event: ' + event + ' emitting to all clients with ID: ' + Socket2ID[id] + " and type receive");
  	for (var socketID of ID2Socket[Socket2ID[id]]) {
      if (Socket2Type[socketID]=="receive") {
        console.log('Emitting to: ' + socketID);
        io.to(socketID).emit(event,data);
      }
  	}
}

function emitToSender(id, event, data) {
  console.log('Receiver sent event: ' + event + ' emitting to all clients with ID: ' + Socket2ID[id] + " and type send");
  for (var socketID of ID2Socket[Socket2ID[id]]) {
    if (Socket2Type[socketID]=="send") {
      console.log('Emitting to: ' + socketID);
      io.to(socketID).emit(event,data);
    }
  }
}