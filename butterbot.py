import discord
import random
import os
import sys
import asyncio
from discord.ext import commands, tasks
from datetime import datetime, timedelta
from collections import deque
from queue import Queue

intent = discord.Intents.all()
intent.message_content = True
bot = commands.Bot(command_prefix="!", intents=intent, help_command=None, case_insensitive=True)

userCooldowns = {}
userOtzAttempts = {}
admins = [202861899098882048, 769230869373124638]
standby = False
messageQueue = Queue()
logfile = f"./logs/log_{datetime.now().strftime("%Y-%m-%d_%H-%M-%S")}.txt"

@tasks.loop(seconds=1)
async def logAsync():
    if messageQueue.empty():
        return
    
    try:
        with open(logfile, 'a') as f:
            f.write(f"[{datetime.now().strftime("%Y-%m-%d %H:%M:%S")}] {messageQueue.get()}\n")
            messageQueue.task_done()
    except:
        return

@bot.event
async def on_ready():
    await bot.guilds[0].afk_channel.connect(reconnect=True)
    await bot.guilds[0].get_channel(1208189932770959400).send("Butter is back online bitches")
    await bot.change_presence(activity=discord.Game("dumme scheiße ab"))
    messageQueue.put("Connected successfully")

    channelInteraction.start()
    soundInteraction.start()
    maintainConnection.start()
    logAsync.start()

@bot.event
async def on_disconnect():
    try:
        for vc in bot.voice_clients:
            await vc.disconnect()
            messageQueue.put("Reconnected")
    except:
        print("disconnect verkackt")
        messageQueue.put("Reconnect failed")

@tasks.loop(minutes=5)
async def channelInteraction():
    messageQueue.put("ChannelInteraction")
    if bot.voice_clients[0].is_playing():
        messageQueue.put("already playing sound")
        return

    if not bot.voice_clients:
        return
    
    # generiere liste aller channels mit leuten drin
    occupiedChannels = []
    for voice_channel in bot.guilds[0].voice_channels:
        if voice_channel.id == bot.guilds[0].afk_channel.id:
            continue
        if voice_channel.members and not all(member == bot.user for member in voice_channel.members):
            occupiedChannels.append(voice_channel)
        
    if not occupiedChannels:
        print("Keine Channel mit Leuten gefunden")
        if bot.voice_clients[0].channel != bot.guilds[0].afk_channel:
            await bot.voice_clients[0].move_to(bot.guilds[0].afk_channel)
            messageQueue.put("Moving to AFK Channel")
        return

    # bot ist schon irgendwo drin
    if bot.voice_clients[0].channel != bot.guilds[0].get_channel(1136646942101880882): 
        if random.random() < 0.1:
            print("Verlasse Channel")
            await bot.voice_clients[0].move_to(bot.guilds[0].afk_channel)
            messageQueue.put("Leaving channel")
            return
        elif random.random() < 0.4 and len(occupiedChannels) > 1:
            choice = random.choice(occupiedChannels)
            print(f"Trete anderem Channel bei: {choice}")
            await bot.voice_clients[0].move_to(choice)
            messageQueue.put(f"Joining different channel: {choice}")
            return

    if random.random() < 0.6:
        choice = random.choice(occupiedChannels)
        print(f"Trete Channel bei: {choice}")
        await bot.voice_clients[0].move_to(choice)
        messageQueue.put(f"Joining channel: {choice}")
        return
    
    print("Mache nix, schlecht gewürfelt")
    messageQueue.put("Bad roll, not doing anything")
 
@tasks.loop(seconds=30)
async def soundInteraction():
    messageQueue.put("SoundInteraction")
    if not bot.voice_clients:
        messageQueue.put("No Connections")
        return
    if bot.voice_clients[0].channel == bot.guilds[0].afk_channel:
        messageQueue.put("Not doing anything, in AFK Channel")
        return
    if bot.voice_clients[0].is_playing():
        messageQueue.put("already playing sound")
        return
    
    soundboard = [f"{os.getcwd()}\\sounds\\{dir}" for dir in os.listdir(f"{os.getcwd()}\\sounds") if os.path.splitext(dir)[1] == ".mp3"]
    soundboard += [f"{os.getcwd()}\\music\\{dir}" for dir in os.listdir(f"{os.getcwd()}\\music") if os.path.splitext(dir)[1] == ".mp3"]

    num = random.random()
    if num < 0.5:
        sound = random.choice(soundboard)
        print(f"\r\nSelected Sound: {sound}\r\n")
        await bot.guilds[0].get_channel(1208189932770959400).send(f"Butter präsentiert: ```{os.path.basename(sound)[:-4]}```")
        messageQueue.put(f"Playing sound: {os.path.basename(sound)[:-4]}")
        bot.voice_clients[0].play(discord.FFmpegPCMAudio(sound))
        while bot.voice_clients[0].is_playing() or bot.voice_clients[0].is_paused():
            await asyncio.sleep(1)
    else:
        print(f"Kein Sound, schlecht gewürfelt ({num})")
        messageQueue.put("Bad roll, not playing anything")

    newinterval = random.randint(5, 1800) 
    print(f"New Soundboard Interval: {newinterval}s, next sound at {(datetime.now() + timedelta(seconds=newinterval)).strftime('%H:%M:%S')}")
    messageQueue.put(f"New Soundboard Interval: {newinterval}s, next sound at {(datetime.now() + timedelta(seconds=newinterval)).strftime('%H:%M:%S')}")
    soundInteraction.change_interval(seconds=newinterval)


@bot.command(name='hilfe')
async def funnyPost_command(ctx):
    await ctx.send("Butter Butter Butter hinter dir Butter")

@bot.command(name='help')
async def help_command(ctx):
    msg = """
Ich kann folgendes:
```
!butter
Ich komm zu dir in den Channel      

!butterbitte
Ich höre vielleicht auf, den aktuellen Sound abzuspielen

!machjetzt
Spiele sofort einen Sound ab (1 Stunde cooldown pro user)

!hilfe
Fuck around and find out

!help
Zeigt diese Hilfe an
```
"""

    await ctx.send(msg)

@bot.command(name='butter', aliases=['kommher', 'bielebiele'])
async def requestJoin_command(ctx):
    messageQueue.put("requestJoin")
    if bot.voice_clients and bot.voice_clients[0].is_playing():
        await ctx.send("Bin beschäftigt")
        messageQueue.put("playing sound, aborting")
        return

    userChannel = ctx.author.voice

    if userChannel == None:
        await ctx.send("Wir treffen uns in \"einfach Quasseln\", ohne treten. Sag nochmal !butter wenn du bereit bist.")
        return 
    
    if userChannel.channel == bot.voice_clients[0].channel:
        with open(os.getcwd() + "/butterdog.jpg", 'rb') as image_file:
            image = discord.File(image_file)
            await ctx.send(file=image)
        return

    await bot.voice_clients[0].move_to(userChannel.channel)
    messageQueue.put(f"Joining channel: {userChannel.channel}")

    
@bot.command(name='butterbitte')
async def attemptStopSound_command(ctx):
    if bot.voice_clients[0].is_playing():
        if random.random() < 0.35:
            await ctx.send("Butter erhört dein Leiden.")
            bot.voice_clients[0].stop()
            messageQueue.put("Stop requested, stopped playing sound")
        else:
            await ctx.send("Frag doch einfach noch mal :)")
            messageQueue.put("Stop requested, did not stop playing sound")
    else:
        await ctx.send("Es läuft kein sound was willst du von mir")
        messageQueue.put("Stop requested, no sound was playing")

def isUserAdmin(ctx) -> bool:
    return ctx.author.id in admins

def isUserAllowed(ctx) -> bool:
    if bot.voice_clients[0].channel == bot.guilds[0].afk_channel:
        return True

    #if ctx.author.id in admin:
    #    return True
    
    usercd = userCooldowns.get(ctx.author.id)
    if usercd is None:
        userCooldowns[ctx.author.id] = datetime.now()
        return True

    delta = datetime.now() - usercd
    if delta > timedelta(minutes=60):
        userCooldowns[ctx.author.id] = datetime.now()
        return True
    else:
        return False

def checkOtzAttempts(ctx) -> bool:
    userID = ctx.author.id

    if userOtzAttempts.get(userID) == None:
        userOtzAttempts[userID] = deque([datetime.now(), None, None])
        return True

    if userOtzAttempts[userID][-1] == None:
        userOtzAttempts[userID].rotate(1)
        userOtzAttempts[userID][0] = datetime.now()
        return True
    
    timeDiff = datetime.now() - userOtzAttempts[userID][-1]
    if timeDiff < timedelta(hours=1):
        return False
    else:
        userOtzAttempts[userID].rotate(1)
        userOtzAttempts[userID][0] = datetime.now()
        return True

def removeLastOtzAttempt(userId: int):
    if userOtzAttempts[userId][1] == None:
        userOtzAttempts.pop(userId)
        return
    
    for i in reversed(range(len(userOtzAttempts[userId]))):
        if userOtzAttempts[userId][i] != None:
            userOtzAttempts[userId][i] = None
            break

@bot.command(name='machjetzt', aliases=['machwas'])
@commands.check(isUserAllowed)
async def makeSound_command(ctx):
    messageQueue.put("makeSound")
    if not bot.voice_clients:
        return
    if bot.voice_clients[0].channel == bot.guilds[0].afk_channel:
        await ctx.send("Bin im afk channel da darf ich nix")
        messageQueue.put(f"In channel: {bot.voice_clients[0].channel}")
        return
    if bot.voice_clients[0].is_playing():
        messageQueue.put("playing sound, aborting")
        await ctx.send("ich mach grad schon was")
        return
    
    soundboard = [f"{os.getcwd()}\\sounds\\{dir}" for dir in os.listdir(f"{os.getcwd()}\\sounds") if os.path.splitext(dir)[1] == ".mp3"]
    soundboard += [f"{os.getcwd()}\\music\\{dir}" for dir in os.listdir(f"{os.getcwd()}\\music") if os.path.splitext(dir)[1] == ".mp3"]
    sound = random.choice(soundboard)
    print(f"\r\nSelected Sound: {sound}\r\n")
    await bot.guilds[0].get_channel(1208189932770959400).send(f"Butter präsentiert: ```{os.path.basename(sound)[:-4]}```")
    messageQueue.put(f"Playing sound: {os.path.basename(sound)[:-4]}")
    bot.voice_clients[0].play(discord.FFmpegPCMAudio(sound))
    while bot.voice_clients[0].is_playing() or bot.voice_clients[0].is_paused():
        await asyncio.sleep(1)

@bot.event
async def on_command_error(ctx, error):
    if isinstance(error, commands.CheckFailure):
        if ctx.invoked_with in ["machjetzt", "machwas", "singwas", "sagwas"]:
            remainingTime = timedelta(minutes=60) - (datetime.now() - userCooldowns.get(ctx.author.id))
            minutes, seconds = divmod(remainingTime.seconds, 60)
            await ctx.send(f"Immer mit der Ruhe du kleiner Pisser. In {minutes}min {seconds}s kannste wieder")
        elif ctx.invoked_with == "otz":
                remainingTime = timedelta(hours=1) - (datetime.now() - userOtzAttempts.get(ctx.author.id)[-1]) 
                minutes, seconds = divmod(remainingTime.seconds, 60)
                await ctx.send(f"Deine mom is ne otze, otz in {minutes}min {seconds}s wieder")
        else:
            await ctx.send("Das darfst du leider nicht :(")
    elif isinstance(error, commands.CommandNotFound):
        await ctx.send(f"```{error}``` kenn ich ni")
    elif isinstance(error, commands.BadArgument):
        await ctx.send(f"{ctx.author.display_name}, ich hoffe du kriegst Husten")
    elif isinstance(error, discord.ClientException):
        await ctx.send("kann nur einen sound auf einmal abspielen sorry")
    else:
        await ctx.send("Irgendwas is passiert, Olli bescheid sagen (╯°□°）╯︵ ┻━┻")
        print(ctx)
        print(error)


@tasks.loop(seconds=5)
async def maintainConnection():
    try:
        test = bot.voice_clients[0].channel
    except IndexError:
        await bot.guilds[0].afk_channel.connect(reconnect=True)
        messageQueue.put("Reconnected")
        print("reconnected")

@bot.command(name="otz")
@commands.check(checkOtzAttempts)
async def otz_command(ctx, userNum: int = None):
    messageQueue.put("otz")
    userID = ctx.author.id
    if userNum == None:
        messageQueue.put("no args")
        await ctx.send("ja was??")
        removeLastOtzAttempt(userID)
        return
    
    if not (1 <= userNum <= 20):
        await ctx.send("Du musst ne zahl zwischen 1 und 20 sagen.")
        removeLastOtzAttempt(userID)
        return
    
    botNum = random.randint(1, 20)
    if botNum == userNum:
        if userCooldowns.get(userID) != None:
            userCooldowns.pop(userID)
        await ctx.send("Gewinne Gewinne Gewinne! Dein !machwas cooldown wurde zurückgesetzt.")
        userOtzAttempts.pop(userID)
        messageQueue.put(f"success, reducing cooldown of {ctx.author.display_name}")
    elif abs(botNum - userNum) == 1:
        await ctx.send(f"{botNum}, darfst nochmal :)")
        messageQueue.put(f"{ctx.author.display_name} was one off, free try")
        removeLastOtzAttempt(userID)
    else:
        await ctx.send(f"Es war {botNum} lol. Womp Womp :(")
        messageQueue.put(f"{ctx.author.display_name} failed")

@bot.command(name="sagwas")
@commands.check(isUserAllowed)
async def onlysounds_command(ctx):
    messageQueue.put("singwas")
    if not bot.voice_clients:
        return
    if bot.voice_clients[0].channel == bot.guilds[0].afk_channel:
        await ctx.send("Bin im afk channel da darf ich nix")
        return
    if bot.voice_clients[0].is_playing():
        await ctx.send("ich sag grad schon was")
    
    soundboard = [f"{os.getcwd()}\\sounds\\{dir}" for dir in os.listdir(f"{os.getcwd()}\\sounds") if os.path.splitext(dir)[1] == ".mp3"]
    
    sound = random.choice(soundboard)
    print(f"\r\nSelected Sound: {sound}\r\n")
    await bot.guilds[0].get_channel(1208189932770959400).send(f"Butter präsentiert: ```{os.path.basename(sound)[:-4]}```")
    bot.voice_clients[0].play(discord.FFmpegPCMAudio(sound))
    messageQueue.put(f"playing {os.path.basename(sound)[:-4]}")
    while bot.voice_clients[0].is_playing() or bot.voice_clients[0].is_paused():
        await asyncio.sleep(1)

@bot.command(name="singwas")
@commands.check(isUserAllowed)
async def onlymusic_command(ctx):
    messageQueue.put("singwas")
    if not bot.voice_clients:
        return
    if bot.voice_clients[0].channel == bot.guilds[0].afk_channel:
        await ctx.send("Bin im afk channel da darf ich nix")
        return
    if bot.voice_clients[0].is_playing():
        await ctx.send("ich sing grad schon was")
    
    soundboard = [f"{os.getcwd()}\\music\\{dir}" for dir in os.listdir(f"{os.getcwd()}\\music") if os.path.splitext(dir)[1] == ".mp3"]
    
    sound = random.choice(soundboard)
    print(f"\r\nSelected Sound: {sound}\r\n")
    await bot.guilds[0].get_channel(1208189932770959400).send(f"Butter präsentiert: ```{os.path.basename(sound)[:-4]}```")
    bot.voice_clients[0].play(discord.FFmpegPCMAudio(sound))
    messageQueue.put(f"playing {os.path.basename(sound)[:-4]}")
    while bot.voice_clients[0].is_playing() or bot.voice_clients[0].is_paused():
        await asyncio.sleep(1)

@bot.command(name="standby")
@commands.check(isUserAdmin)
async def standby_command(ctx):
    global standby
    if not bot.voice_clients:
        return
    if bot.voice_clients[0].is_playing():
        ctx.send("ich mach grad was, moment")

    if standby:
        await ctx.send("bin schon im standby")
        return

    channelInteraction.cancel()
    soundInteraction.cancel()

    while channelInteraction.is_running() or soundInteraction.is_running():
        await asyncio.sleep(0.5)
    standby = True

    if bot.voice_clients[0].channel != bot.guilds[0].afk_channel:
        await bot.voice_clients[0].move_to(bot.guilds[0].afk_channel)

    await ctx.send("ok")
    messageQueue.put("standby")

@bot.command(name="wakeup")
@commands.check(isUserAdmin)
async def standby_command(ctx):
    global standby
    if not bot.voice_clients:
        return
    if bot.voice_clients[0].is_playing():
        ctx.send("ich mach grad was, moment")

    if not standby:
        await ctx.send("ich bin nicht im standby was willst du")
        return

    channelInteraction.start()
    soundInteraction.start()
    standby = False

    with open(os.getcwd() + "/wakeup.gif", 'rb') as image_file:
        image = discord.File(image_file)
        await ctx.send(file=image)

    messageQueue.put("wakeup")

@bot.command(name="reboot", aliases=['restart', 'lüge'])
@commands.check(isUserAdmin)
async def reboot_command(ctx):
    messageQueue.put("rebooting")
    if ctx.invoked_with == "lüge":
        await ctx.send("hast recht, komme gleich wieder...")
    else:
        await ctx.send("starte neu...")
    await bot.close()
    os.execv(sys.executable, ['python'] + sys.argv)

@bot.command(name="shutdown", aliases=['kill'])
@commands.check(isUserAdmin)
async def shutdown_command(ctx):
    messageQueue.put("shutting down")
    await ctx.send("aus die maus, ich bin jetzt raus")
    await bot.close()

# bot starten
with open("token.txt") as h:
    bot.run(h.readline())