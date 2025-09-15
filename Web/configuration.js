// كشف تلقائي للأجهزة عند تحميل الصفحة
document.addEventListener('DOMContentLoaded', function() {
    refreshDevices();
    loadChannels();
});

// تحديث قائمة الأجهزة
async function refreshDevices() {
    try {
        const videoResponse = await fetch('/DirectShow/Devices/Video');
        const videoDevices = await videoResponse.json();
        
        const audioResponse = await fetch('/DirectShow/Devices/Audio');
        const audioDevices = await audioResponse.json();
        
        displayDevices(videoDevices, 'videoDevices');
        populateSelect(videoDevices, 'videoDevice');
        displayDevices(audioDevices, 'audioDevices');
        populateSelect(audioDevices, 'audioDevice');
    } catch (error) {
        console.error('Error refreshing devices:', error);
        alert('Error refreshing devices');
    }
}

// عرض الأجهزة في القوائم
function displayDevices(devices, elementId) {
    const element = document.getElementById(elementId);
    if (devices.length === 0) {
        element.innerHTML = '<div class="text-muted">No devices found</div>';
        return;
    }
    
    element.innerHTML = devices.map(device => 
        `<div class="form-check">
            <input class="form-check-input" type="radio" name="${elementId}" value="${device}">
            <label class="form-check-label">${device}</label>
        </div>`
    ).join('');
}

// تعبئة عناصر Select بالأجهزة
function populateSelect(devices, selectId) {
    const select = document.getElementById(selectId);
    select.innerHTML = devices.map(device => 
        `<option value="${device}">${device}</option>`
    ).join('');
    
    if (devices.length === 0) {
        select.innerHTML = '<option value="">No devices available</option>';
    }
}

// تحميل القنوات المُكونة
async function loadChannels() {
    try {
        const response = await fetch('/DirectShow/Channels');
        const channels = await response.json();
        displayChannels(channels);
    } catch (error) {
        console.error('Error loading channels:', error);
        document.getElementById('channelsList').innerHTML = 
            '<div class="alert alert-danger">Error loading channels</div>';
    }
}

// عرض القنوات
function displayChannels(channels) {
    const container = document.getElementById('channelsList');
    
    if (channels.length === 0) {
        container.innerHTML = '<div class="alert alert-info">No channels configured</div>';
        return;
    }
    
    container.innerHTML = channels.map(channel => `
        <div class="card channel-card">
            <div class="card-body">
                <h5 class="card-title">${channel.name}</h5>
                <p class="card-text">
                    <strong>Video:</strong> ${channel.videoDevice}<br>
                    <strong>Audio:</strong> ${channel.audioDevice || 'None'}<br>
                    <strong>Resolution:</strong> ${channel.resolution} | 
                    <strong>Framerate:</strong> ${channel.framerate}fps<br>
                    <strong>Bitrate:</strong> Video: ${channel.videoBitrate}bps | Audio: ${channel.audioBitrate}bps
                </p>
                <div class="btn-group">
                    <button class="btn btn-primary btn-sm" onclick="startStream('${channel.id}')">Start Stream</button>
                    <button class="btn btn-warning btn-sm" onclick="stopStream('${channel.id}')">Stop Stream</button>
                    <button class="btn btn-danger btn-sm" onclick="deleteChannel('${channel.id}')">Delete</button>
                </div>
            </div>
        </div>
    `).join('');
}

// إضافة قناة جديدة
document.getElementById('channelForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const channelData = {
        name: document.getElementById('channelName').value,
        videoDevice: document.getElementById('videoDevice').value,
        audioDevice: document.getElementById('audioDevice').value,
        resolution: document.getElementById('resolution').value,
        framerate: parseInt(document.getElementById('framerate').value),
        videoBitrate: parseInt(document.getElementById('videoBitrate').value),
        audioBitrate: parseInt(document.getElementById('audioBitrate').value)
    };
    
    try {
        const response = await fetch('/DirectShow/Channels', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(channelData)
        });
        
        if (response.ok) {
            alert('Channel added successfully');
            document.getElementById('channelForm').reset();
            loadChannels();
        } else {
            alert('Error adding channel');
        }
    } catch (error) {
        console.error('Error adding channel:', error);
        alert('Error adding channel');
    }
});

// بدء البث
async function startStream(channelId) {
    try {
        const response = await fetch(`/DirectShow/Stream/Start/${channelId}`, {
            method: 'POST'
        });
        
        if (response.ok) {
            const result = await response.json();
            alert(`Stream started: ${result.streamUrl}`);
        } else {
            alert('Error starting stream');
        }
    } catch (error) {
        console.error('Error starting stream:', error);
        alert('Error starting stream');
    }
}

// إيقاف البث
async function stopStream(channelId) {
    try {
        const response = await fetch(`/DirectShow/Stream/Stop/${channelId}`, {
            method: 'POST'
        });
        
        if (response.ok) {
            alert('Stream stopped');
        } else {
            alert('Error stopping stream');
        }
    } catch (error) {
        console.error('Error stopping stream:', error);
        alert('Error stopping stream');
    }
}

// حذف قناة
async function deleteChannel(channelId) {
    if (!confirm('Are you sure you want to delete this channel?')) {
        return;
    }
    
    try {
        const response = await fetch(`/DirectShow/Channels/${channelId}`, {
            method: 'DELETE'
        });
        
        if (response.ok) {
            alert('Channel deleted');
            loadChannels();
        } else {
            alert('Error deleting channel');
        }
    } catch (error) {
        console.error('Error deleting channel:', error);
        alert('Error deleting channel');
    }
}