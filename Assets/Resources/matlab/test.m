[s, fs] = audioread('up_left_2.m4a');
s = s(:,1); % channel 0 (col 0)
i1 = 8e4; % just fucking 80000
i2 = 3.57e5; % just fucking 357000
s = s(i1:i2); % in range of i1 and i2
s = decimate(s, 2);
fs = fs/2;
n = size(s,1);
t = (0 : n-1)'/fs;
% plot(t, s);
% plot(s);


% soundsc(s, fs);
nfft = 2048;
tic;
[S,F,T] = spectrogram(s, nfft, 0, nfft, fs);
toc;
surf( T, F, log(abs(S)) );
xlabel('T');
ylabel('F');
view(0,90);
shading flat;

S = abs(S);
soundsc( s( t < 4 & t>2.5 ) , fs ) % scale and then play audio

s1 = s;
[f0_time,f0_value,SHR,f0_candidates]=... % continue next line
    shrp(s1,fs,[50 250], [], [], 0.2);
figure;
subplot(2,1,1);
plot(f0_time, f0_value);
title('pitch');
f0_time = f0_time/1000;
m = length(f0_time);
amp = zeros(m,1);
for i=1:m
    [~,k] = min(abs(f0_time(i) - T));
    A = abs(S(:,k)).^2;
    fq = f0_value(i) * (1:25);
    [~,k] = min(abs(fq - F),[],1);
    amp(i) = sum(A(k));
end
subplot(2,1,2);
plot(f0_time, log(amp));
title('amp');