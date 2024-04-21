import discord
import random
import os
import asyncio
from discord.ext import commands, tasks
from datetime import datetime, timedelta

intent = discord.Intents.all()
intent.message_content = True
bot = commands.Bot(command_prefix="!", intents=intent, help_command=None)

userCooldowns = {}

@bot.event
async def on_ready():
    await bot.guilds[0].afk_channel.connect(reconnect=True)
    await bot.guilds[0].get_channel(1208189932770959400).send("Butter is back online bitches")

    channelInteraction.start()
    soundInteraction.start()
    maintainConnection.start()

@bot.event
async def on_disconnect():
    try:
        for vc in bot.voice_clients:
            await vc.disconnect()
    except:
        print("disconnect verkackt")

@tasks.loop(minutes=5)
async def channelInteraction():
    # generiere liste aller channels mit leuten drin
    occupiedChannels = []
    for voice_channel in bot.guilds[0].voice_channels:
        if voice_channel.id == bot.guilds[0].afk_channel.id:
            continue
        if voice_channel.members:
            occupiedChannels.append(voice_channel)
        
    if not occupiedChannels:
        print("Keine Channel mit Leuten gefunden")
        await bot.voice_clients[0].move_to(bot.guilds[0].afk_channel)
        return

    # bot ist schon irgendwo drin
    if bot.voice_clients[0].channel != bot.guilds[0].get_channel(1136646942101880882): 
        if random.random() < 0.4:
            print("Verlasse Channel")
            await bot.voice_clients[0].move_to(bot.guilds[0].afk_channel)
            return
        elif random.random() < 0.4 and len(occupiedChannels) > 1:
            choice = random.choice(occupiedChannels)
            print(f"Trete anderem Channel bei: {choice}")
            await bot.voice_clients[0].move_to(choice)
            return

    if random.random() < 0.6:
        choice = random.choice(occupiedChannels)
        print(f"Trete Channel bei: {choice}")
        await bot.voice_clients[0].move_to(choice)
        return
    
    print("Mache nix, schlecht gewürfelt")
 
@tasks.loop(seconds=30)
async def soundInteraction():
    if not bot.voice_clients:
        return
    if bot.voice_clients[0].channel == bot.guilds[0].afk_channel:
        return
    if bot.voice_clients[0].is_playing():
        return
    
    soundboard = [f"{os.getcwd()}\\sounds\\{dir}" for dir in os.listdir(f"{os.getcwd()}\\sounds") if os.path.splitext(dir)[1] == ".mp3"]

    num = random.random()
    if num < 0.5:
        sound = random.choice(soundboard)
        print(f"\r\nSelected Sound: {sound}\r\n")
        await bot.guilds[0].get_channel(1208189932770959400).send(f"Butter präsentiert: ```{os.path.basename(sound)[:-4]}```")
        bot.voice_clients[0].play(discord.FFmpegPCMAudio(sound))
        while bot.voice_clients[0].is_playing() or bot.voice_clients[0].is_paused():
            await asyncio.sleep(1)
    else:
        print(f"Kein Sound, schlecht gewürfelt ({num})")

    newinterval = random.randint(5, 1800) 
    print(f"New Soundboard Interval: {newinterval}s, next sound at {(datetime.now() + timedelta(seconds=newinterval)).strftime('%H:%M:%S')}")
    soundInteraction.change_interval(seconds=newinterval)


@bot.command(name='hilfe')
async def funnyPost_command(ctx):
    help_msg = "Butter Butter Butter hinter dir Butter"
    await ctx.send(help_msg)

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
    if bot.voice_clients and bot.voice_clients[0].is_playing():
        await ctx.send("Bin beschäftigt")
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

    
@bot.command(name='butterbitte', aliases=['ButterBitte', 'bittebutter', 'BitterButter'])
async def attemptStopSound_command(ctx):
    if bot.voice_clients[0].is_playing():
        if random.random() < 0.35:
            await ctx.send("Butter erhört dein Leiden.")
            bot.voice_clients[0].stop()
        else:
            await ctx.send("Frag doch einfach noch mal :)")
    else:
        await ctx.send("Es läuft kein sound was willst du von mir")

def isUserAllowed(ctx):
    if bot.voice_clients[0].channel == bot.guilds[0].afk_channel:
        return True

    if ctx.author.id == 202861899098882048: # olli darf das :)
        return True
    
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

@bot.command(name='machjetzt', aliases=['machwas'])
@commands.check(isUserAllowed)
async def makeSound_command(ctx):
    if not bot.voice_clients:
        return
    if bot.voice_clients[0].channel == bot.guilds[0].afk_channel:
        await ctx.send("Bin im afk channel da darf ich nix")
        return
    
    soundboard = [f"{os.getcwd()}\\sounds\\{dir}" for dir in os.listdir(f"{os.getcwd()}\\sounds") if os.path.splitext(dir)[1] == ".mp3"]
    sound = random.choice(soundboard)
    print(f"\r\nSelected Sound: {sound}\r\n")
    await bot.guilds[0].get_channel(1208189932770959400).send(f"Butter präsentiert: ```{os.path.basename(sound)[:-4]}```")
    bot.voice_clients[0].play(discord.FFmpegPCMAudio(sound))
    while bot.voice_clients[0].is_playing() or bot.voice_clients[0].is_paused():
        await asyncio.sleep(1)

@bot.event
async def on_command_error(ctx, error):
    if isinstance(error, commands.CheckFailure):
        if ctx.invoked_with == "machjetzt" or ctx.invoked_with == "machwas":
            remainingTime = timedelta(minutes=60) - (datetime.now() - userCooldowns.get(ctx.author.id))
            minutes, seconds = divmod(remainingTime.seconds, 60)
            await ctx.send(f"Immer mit der Ruhe du kleiner Pisser. In {minutes}min {seconds}s kannste wieder")
        else:
            await ctx.send("Das darfst du leider nicht :(")
    elif isinstance(error, commands.CommandNotFound):
        await ctx.send(f"```{error}``` kenn ich ni")
    else:
        await ctx.send("Irgendwas is passiert, Olli bescheid sagen (╯°□°）╯︵ ┻━┻")
        print(ctx)
        print(error)


@tasks.loop(seconds=5)
async def maintainConnection():
    try:
        #print(f"Channel: {bot.voice_clients[0].channel}")
        test = bot.voice_clients[0].channel
    except IndexError:
        await bot.guilds[0].afk_channel.connect(reconnect=True)
        print("reconnected")

# bot starten
with open("token.txt") as h:
    bot.run(h.readline())