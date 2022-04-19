const starfieldArea = document.getElementById('starfield-area');
const optionsPanel = document.getElementById('options-panel');
const horzBanner = document.getElementById('horz-banner');
const canvas = document.getElementById('my-canvas');
const planetDescriptionBox = document.getElementById('planet-description-box')
const ctx = canvas.getContext('2d');

const planetSize=20
let starfieldZoom = 200, starfieldX = 0, starfieldY = 0, starfieldXPercent = 50, starfieldYPercent = 50, starfieldXStartingPercent, starfieldYStartingPercent, minimumZoom=100, planetDescriptionOffset=30, scaledPlanetSize=20;
let maxSelectDistance = 30, playerID;
let locationsRoot;
let closePlanetData = null;
let world; /*{galaxyName,planets:[
    {x:0,y:0,name:"UL",description:"Zoinks",factionColor:"#FFFFFF",isImportant:true,less:null,greater:null},
    {x:3840,y:0,name:"UR",description:"Gasp!",factionColor:"#FFFFFF",isImportant:true,less:null,greater:null},
    {x:3840,y:2160,name:"LR",description:"Zounds",factionColor:"#FFFFFF",isImportant:false,less:null,greater:null},
    {x:0,y:2160,name:"LL",description:"Gadzooks",factionColor:"#FFFFFF",isImportant:true,less:null,greater:null},
    {x:1920,y:0,name:"UC",description:"Egads",factionColor:"#FFFFFF",isImportant:true,less:null,greater:null}],
    connections,orders, currentPlayers,maxPlayers};*/
let factionColorPairs = [];

let ourRequest = new XMLHttpRequest();
ourRequest.onerror = function(){
    console.log(ourRequest.responseText);
}
ourRequest.open('GET', 'TODO URL');
ourRequest.setRequestHeader('Authorization', 'Bearer ' + sessionStorage.getItem("JWT"));
ourRequest.onLoad = function() {
    world = JSON.parse(ourRequest.responseText);
    let colorIterator = 0;
    if(world.currentPlayers != world.maxPlayers){
        world.players.forEach(player =>{
            player.factionColor = getFactionColor(colorIterator);
            colorIterator++;
        });
    }else{
        openStarfield();
    }
}
ourRequest.send();

locationsRoot = sortLocations(locations);

function openStarfield(){
    console.log("Opening Starfield");
    document.getElementById('starfield-div').classList.remove('hidden');
    document.getElementById('landing-div').classList.add('hidden');

    window.addEventListener('resize', fixMinmumZoom);
    window.addEventListener('resize', fixCanvasSize);
    canvas.addEventListener('mousemove', checkHover);
    scaledPlanetSize = Math.floor(starfieldZoom*planetSize/100);

    fixCanvasSize();
    fixMinmumZoom();

    canvas.addEventListener('wheel', event => {
        starfieldZoom = Math.max(Math.min(starfieldZoom - Math.sign(event.deltaY)*5, 300),minimumZoom);
        canvas.style.backgroundSize = starfieldZoom + '%';
        scaledPlanetSize = Math.floor(starfieldZoom*planetSize/100);
        drawCanvas();
    })
    
    canvas.addEventListener('mousedown', event => {
        //Check if click is close to system
        let tf = false
        for(let i=0; i<locations.length;i++){
            if(Math.abs(locations[i].x - event.pageX) + Math.abs(locations[i] - event.pageY) < maxSelectDistance){

            }
        }
        starfieldX = event.pageX;
        starfieldY = event.pageY;
        starfieldYStartingPercent = starfieldYPercent;
        starfieldXStartingPercent = starfieldXPercent;
        canvas.addEventListener('mouseup', starfieldDrop);
        canvas.addEventListener('mouseout', starfieldDrop);
        canvas.addEventListener('mousemove', starfieldDrag);
        console.log('down');
    })
}

function starfieldDrag(event){
    starfieldYPercent = Math.max(Math.min((starfieldY-event.pageY)/3 + starfieldYStartingPercent,100),0);
    starfieldXPercent = Math.max(Math.min((starfieldX-event.pageX)/10 + starfieldXStartingPercent,100),0);
    canvas.style.backgroundPosition = starfieldXPercent.toString() + "% " + starfieldYPercent.toString() + "%";
    console.log('drag');
    drawCanvas();
}

function starfieldDrop(event){
    canvas.removeEventListener('mousemove', starfieldDrag);
    canvas.removeEventListener('mouseup', starfieldDrop);
    canvas.removeEventListener('mouseout', starfieldDrop);
    console.log('up');
}

function fixMinmumZoom(){
    minimumZoom = Math.max (178*(canvas.offsetHeight/canvas.offsetWidth),100);

    starfieldZoom = Math.max(starfieldZoom,minimumZoom);
    canvas.style.backgroundSize = starfieldZoom + '%';
    console.log(178*(canvas.offsetHeight/canvas.offsetWidth));
}

function fixCanvasSize(){

    canvas.height = window.innerHeight-horzBanner.offsetHeight;
    canvas.width = window.innerWidth - optionsPanel.offsetWidth;
    console.log(canvas.height,canvas.width,window.innerHeight,window.innerWidth);
    drawCanvas();
}

function bgSpaceToScreenSpace(x,y){
    let retval = [Math.floor(((x/3840)+((x/3840)-starfieldXPercent/100)*((starfieldZoom-minimumZoom)/minimumZoom))*canvas.width),
    Math.floor(((y/2160)+((y/2160)-starfieldYPercent/100)*((starfieldZoom-minimumZoom)/minimumZoom))*canvas.height)];
    return retval;
}

function drawCanvas(){
    ctx.clearRect(0,0,canvas.width,canvas.height);
    ctx.strokeStyle = '#FFFFFF';
    ctx.fillStyle = '#FF0000';
    ctx.lineWidth = 3;
    ctx.font = "30px Arial";
    ctx.save();

    world.connections.forEach(element => {
        let screenSpacePosStart = bgSpaceToScreenSpace(element.startX,e,element.startY);
        let screenSpacePosEnd = bgSpaceToScreenSpace(element.endX,e,element.endY);
        //Note this doesn't catch all of the unimportant lines. Example missed line: starts above and centered and ends centered and to the right.
        if((screenSpacePosStart[0] < 0 && screenSpacePosEnd[0] < 0) || (screenSpacePosStart[1] < 0 && screenSpacePosEnd[1] < 0) || (screenSpacePosStart[0] > canvas.width && screenSpacePosEnd[0] > canvas.width) || (screenSpacePosStart[1] > canvas.height && screenSpacePosEnd[1] > canvas.height)){
            ctx.beginPath(screenSpacePos[0],screenSpacePos[1]);
            ctx.lineTo(endX,screenSpacePosEnd[1]);
            ctx.stroke();
        }
    })
    ctx.restore();

    world.orders.forEach(element => {
        ctx.save();
        var l1 = world.planets[element.location1];
        switch(element.order){
            case (0)://Move
                var l2 = world.planets[element.location2];
                arrow(ctx, bgSpaceToScreenSpace(l1.x,l1.y), bgSpaceToScreenSpace(l2.x,l2.y))
                break;
            case (1)://Secure
            var l2 = world.planets[element.location2];
            var l3 = world.planets[element.location3];
                break;
            case (2)://Create
                ctx.fillStyle = "4444FF";
                ctx.beginPath();
                ctx.rect(l1.x-planetSize-3,l1.y-planetSize-3,planetSize+6,planetSize+6);
                ctx.stroke();
                break;
            case (3)://Destroy
                ctx.fillStyle = "#FF4444";
                ctx.beginPath();
                ctx.rect(l1.x-planetSize-3,l1.y-planetSize-3,planetSize+6,planetSize+6);
                ctx.stroke();
                break;
        }
        ctx.restore();
    });

    world.planets.forEach(element => {
        ctx.save();
        let screenSpacePos = bgSpaceToScreenSpace(element.x,element.y);
        //If the element is on screen or almost on screen
        if(!(screenSpacePos[0] + scaledPlanetSize < 0 || screenSpacePos[1] + scaledPlanetSize < 0 || screenSpacePos[0] - scaledPlanetSize > canvas.width || screenSpacePos[1] - scaledPlanetSize > canvas.height)){
            //Draw it
            ctx.fillStyle = element.factionColor;
            ctx.beginPath();
            ctx.arc(screenSpacePos[0], screenSpacePos[1], scaledPlanetSize, 0, 2 * Math.PI);
            ctx.fill()
            ctx.stroke();

            if(element.isImportant){
                ctx.fillStyle = '#000000';
                ctx.fillText("'\u269D'",screenSpacePos[0],screenSpacePos[1])//229B may be a better darker choice. 2605 even more so
            }
        }
        ctx.restore();
    });

}

//From https://stackoverflow.com/questions/8211745/draw-an-arrow-on-html5-canvas-between-two-objects
//Answer by Phrogz
function arrow(ctx,p1,p2,size){
    ctx.save();

    var points = edges(ctx,p1,p2);
    if (points.length < 2) return 
    p1 = points[0], p2=points[points.length-1];

    // Rotate the context to point along the path
    var dx = p2.x-p1.x, dy=p2.y-p1.y, len=Math.sqrt(dx*dx+dy*dy);
    ctx.translate(p2.x,p2.y);
    ctx.rotate(Math.atan2(dy,dx));

    // line
    ctx.lineCap = 'round';
    ctx.beginPath();
    ctx.moveTo(0,0);
    ctx.lineTo(-len,0);
    ctx.closePath();
    ctx.stroke();

    // arrowhead
    ctx.beginPath();
    ctx.moveTo(0,0);
    ctx.lineTo(-size,-size);
    ctx.lineTo(-size, size);
    ctx.closePath();
    ctx.fill();

    ctx.restore();
  }
//TODO Make sure this can't happen in the middle of a click and drag
function openPlanetDescription(x,y,name,description){
    planetDescriptionBox.classList.remove("hidden");
    if(x*2 > window.width ){
        if(y*2 > window.height){
            planetDescriptionBox.style.bottom = null;
            planetDescriptionBox.style.left = null;
            planetDescriptionBox.style.right = x-planetDescriptionOffset;
            planetDescriptionBox.style.top = y;
        }else{
            planetDescriptionBox.style.bottom = y;
            planetDescriptionBox.style.left = null;
            planetDescriptionBox.style.right = x-planetDescriptionOffset;
            planetDescriptionBox.style.top = null;
        }
    }else{
        if(y*2 > window.height){
            planetDescriptionBox.style.bottom = x-planetDescriptionOffset;
            planetDescriptionBox.style.left = null;
            planetDescriptionBox.style.right = null;
            planetDescriptionBox.style.top = y;
        }else{
            planetDescriptionBox.style.bottom = x-planetDescriptionOffset;
            planetDescriptionBox.style.left = null;
            planetDescriptionBox.style.right = null;
            planetDescriptionBox.style.top = null;
        }
    }
    planetDescriptionBox.children[0].innerHTML = name;
    planetDescriptionBox.children[1].innerHTML = description;
}

function closePlanetDescription(){
    if(!planetDescriptionBox.classList.contains("hidden"))
        planetDescriptionBox.classList.add("hidden");
}

function checkHover(event){
    let canvasX = event.pageX-canvas.offsetLeft;
    let canvasY = event.pageY-canvas.offsetTop;
    if(closePlanetData != null){
        //console.log(Math.abs(closePlanetData[0].screenX-canvasX)+Math.abs(closePlanetData[0].screenY-canvasY), maxSelectDistance*starfieldZoom*.1);
        if(Math.abs(closePlanetData.screenX-canvasX)+Math.abs(closePlanetData.screenY-canvasY) > maxSelectDistance*starfieldZoom*.01){
            closePlanetData = null;
            closePlanetDescription();
        }
    }else{
        closePlanetData = findClosePlanetRecursive(canvasX,canvasY,locationsRoot,0);
        if(closePlanetData != null){
            openPlanetDescription(event.pageX,event.pageY,closePlanetData.name,closePlanetData.description);
        }
    }
}
function sortLocations(locationList){
    let sortedXList = locationList.map((x) =>x);
    let sortedYList = locationList.map((x) =>x);
    sortedXList.sort( (a,b) => {
        return a.x - b.x;
    })
    sortedYList.sort( (a,b) => {
        return a.y - b.y;
    })
    return recursiveLocationSort(sortedXList,sortedYList,false)
}

function recursiveLocationSort(subListX,subListY, isVertical){
    console.log(subListX,subListX.length,subListY,subListY.length,isVertical);
    if(subListX.length==0) return null;
    if(subListY.length==0) return null;
    if(isVertical){
        let leftY = subListY.slice(0,Math.floor(subListY.length/2));
        let rightY = subListY.slice(Math.floor(subListY.length/2)+1,subListY.length);
        let localRoot = subListY[Math.floor(subListY.length/2)];

        localRoot.left = recursiveLocationSort(leftY, subListX.filter( (x) => {
            return leftY.includes(x);
        }),false);

        localRoot.right = recursiveLocationSort(rightY, subListX.filter( (x) => {
            return rightY.includes(x);
        }),false);
        return localRoot;

    }else{
        let leftX = subListX.slice(0,Math.floor(subListX.length/2));
        let rightX = subListX.slice(Math.floor(subListX.length/2)+1,subListX.length);
        let localRoot = subListX[Math.floor(subListX.length/2)];

        localRoot.left = recursiveLocationSort(leftX, subListY.filter( (x) => {
            return leftX.includes(x);
        }),true)

        localRoot.right = recursiveLocationSort(rightX, subListY.filter( (x) => {
            return rightX.includes(x);
        }),true)
        return localRoot;
    }
}

function findClosePlanetRecursive(screenX,screenY,currentRoot,depth){
    if(currentRoot == null) return null;
    if(Math.abs(currentRoot.screenX-screenX) + Math.abs(currentRoot.screenY-screenY) < maxSelectDistance*starfieldZoom*.01)
        return currentRoot;
    if(depth%2==0){
        if(screenX < currentRoot.screenX)
            return findClosePlanetRecursive(screenX,screenY,currentRoot.left,depth+1);
        return findClosePlanetRecursive(screenX,screenY,currentRoot.right,depth+1);
    }else{
        if(screenY < currentRoot.screenY)
            return findClosePlanetRecursive(screenX,screenY,currentRoot.left,depth+1);
        return findClosePlanetRecursive(screenX,screenY,currentRoot.right,depth+1);
    }
}

/*
window.addEventListener('keydown', (event) =>{
    if(event.code === 'Space'){
        recursiveLog(locationsRoot,0);
    }
})

function recursiveLog(planet,depth){
    if(planet != null){
        recursiveLog(planet.left,depth+1)
        console.log(planet.name,depth);
        recursiveLog(planet.right,depth+1);
    }
}
*/


function getFactionColor(colorIterator){
    switch(colorIterator){
        case 0:
            return "#FF0000";//Red
        case 1:
            return "#FFE9a5";//yellow
        case 2:
            return "#00FF00";//Green
        case 3:
            return "#ed54ba";//pink
        case 4:
            return "#0000FF";//Blue
        case 5:
            return "#bb00FF";//Purple
        case 6:
            return "#cbff00";//lime
        case 7:
            return "#f6b26b";//orangey
        case 8:
            return "#2b4982";//dark blue
        case 9:
            return "#3e5622";//dark green
        case 10:
            return "#bcbcbc";//grey
        case 11:
            return "#ce7e00";//brown
        default:
            return "#FFFFFF";
    }
}
